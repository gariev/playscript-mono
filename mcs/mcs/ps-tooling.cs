using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Mono.CSharp;

namespace Mono.PlayScript.Tooling
{

	public class FieldInfo
	{
		public TypeInfo ContainingType;
		public Field BackingField;
		public string Name;
		public string Help;
		public string Category;
		public int Start;
		public int Length;
		public bool IsSparse;

		protected TypeSpec _fieldType;
		protected TypeInfo _fieldTypeInfo;

		public virtual MemberCore Member {
			get { return null; }
		}

		public TypeSpec FieldType {
			get {
				return _fieldType;
			}
		}

		public virtual void SetFieldType(TypeSpec fieldType)
		{
			_fieldType = fieldType;
			if (_fieldType.MemberDefinition is ClassOrStruct) {
				var assetClass = _fieldType.MemberDefinition as ClassOrStruct;
				if (!Tooling.AssetTypeInfosByTypeDef.ContainsKey (assetClass)) {
					if (!Tooling.AssetClassesToAdd.Contains (assetClass)) {
						Tooling.AssetClassesToAdd.Add (assetClass);
					}
				}
			}
		}

		public TypeInfo FieldTypeInfo {
			get {
				if (_fieldTypeInfo == null && _fieldType.MemberDefinition is ClassOrStruct) {
					_fieldTypeInfo = Tooling.AssetTypeInfosByTypeDef[_fieldType.MemberDefinition as ClassOrStruct];
				}
				return _fieldTypeInfo;
			}
		}
	}

	public class FieldFieldInfo : FieldInfo
	{
		public override MemberCore Member {
			get { return BackingField; }
		}

		public FieldFieldInfo(TypeInfo containingType, Field field)
		{
			ContainingType = containingType;
			BackingField = field;
			SetFieldType(field.TypeExpression.ResolveAsType(field.Parent));
			Name = field.Name;
		}
	}

	public class PropertyFieldInfo : FieldInfo
	{
		public Property Property;

		public override MemberCore Member {
			get { return Property; }
		}

		public PropertyFieldInfo(TypeInfo containingType, Property prop)
		{
			ContainingType = containingType;
			Property = prop;
			SetFieldType(prop.TypeExpression.ResolveAsType(prop.Parent));
			if (prop.Get == null) {
				prop.Compiler.Report.Error (7823, prop.Location, "Asset class/struct properties must declare a getter.");
			}
			Name = prop.ShortName;
		}
	}

	/// <summary>
	/// The type of collection accessor method.
	/// </summary>
	public enum CollectionAccessorType {
		/// <summary>Method which adds an element to the collection.</summary>
		Add,
		/// <summary>Method which removes an element from the collection at a given index.</summary>
		RemoveAt,
		/// <summary>Method which gets an element from the collection.</summary>
		Get,
		/// <summary>Method which sets an element in the collection.</summary>
		Set,
		/// <summary>Method which inserts an element into the collection.</summary>
		Insert,
		/// <summary>Method which returns the index of an element in the collection.</summary>
		IndexOf,
		/// <summary>Method which returns true if an element is contained in the collection.</summary>
		Initialize
	}

	public class CollectionFieldInfo : FieldInfo
	{
		public Method AddMethod;
		public Method RemoveAtMethod;
		public Method GetMethod;
		public Method SetMethod;
		public Method InsertMethod;
		public Method IndexOfMethod;
		public Method InitializeMethod;

		public Method AddWrapperMethod;
		public Method RemoveAtWrapperMethod;
		public Method SetWrapperMethod;
		public Method InsertWrapperMethod;
		public Method IndexOfWrapperMethod;
		public Method InitializeWrapperMethod;

		public int ElementParam;
		public TypeSpec ElementType;

		public CollectionFieldInfo(TypeInfo containingType, string name, int elemParam)
		{
			ContainingType = containingType;
			Name = name;
			ElementParam = elemParam;
		}

		public void SetAccessorMethod(CollectionAccessorType accessorType, Method method)
		{
			switch (accessorType) {
			case CollectionAccessorType.Add:
				AddMethod = method;
				TrySetFieldTypeFromParam (method, ElementParam);
				break;
			case CollectionAccessorType.RemoveAt:
				RemoveAtMethod = method;
				TrySetFieldTypeFromParam (method, ElementParam);
				break;
			case CollectionAccessorType.Get:
				GetMethod = method;
				TrySetFieldTypeFromParam (method, -1);
				break;
			case CollectionAccessorType.Set:
				SetMethod = method;
				TrySetFieldTypeFromParam (method, ElementParam);
				break;
			case CollectionAccessorType.Insert:
				InsertMethod = method;
				TrySetFieldTypeFromParam (method, ElementParam);
				break;
			case CollectionAccessorType.IndexOf:
				IndexOfMethod = method;
				TrySetFieldTypeFromParam (method, ElementParam);
				break;
			case CollectionAccessorType.Initialize:
				InitializeMethod = method;
				TypeSpec vectorTypeSpec = ((Parameter)method.ParameterInfo [ElementParam]).TypeExpression.ResolveAsType (method.Parent);
				SetFieldType (vectorTypeSpec);
				break;
			}
		}

		private void TrySetFieldTypeFromParam(Method method, int paramIdx)
		{
			TypeSpec elemTypeSpec = null;
			if (paramIdx == -1) {
				if (elemTypeSpec.Kind == MemberKind.Void) {
					method.Compiler.Report.Error (7821, method.Location, "Collection accessor method must not be void");
				} else {
					elemTypeSpec = method.TypeExpression.ResolveAsType (method.Parent);
				}
			} else if (method.ParameterInfo.Types.Length > paramIdx) {
				if (paramIdx >= method.ParameterInfo.FixedParameters.Length) {
					method.Compiler.Report.Error (7820, method.Location, "Collection accessor method missing element paramter");
				} else {
					elemTypeSpec = ((Parameter)method.ParameterInfo [paramIdx]).TypeExpression.ResolveAsType (method.Parent);
				}
			}

			ElementType = elemTypeSpec;

			if (elemTypeSpec != null) {

				if (elemTypeSpec.MemberDefinition is ClassOrStruct) {
					var assetClass = elemTypeSpec.MemberDefinition as ClassOrStruct;
					if (!Tooling.AssetTypeInfosByTypeDef.ContainsKey (assetClass)) {
						if (!Tooling.AssetClassesToAdd.Contains (assetClass)) {
							Tooling.AssetClassesToAdd.Add (assetClass);
						}
					}
				}

				TypeSpec vectorType = method.Module.PredefinedTypes.AsVector.Resolve ().MakeGenericType (method.Parent, new [] { elemTypeSpec });
				SetFieldType (vectorType);
			}
		}

		public override void SetFieldType(TypeSpec type)
		{
			if (FieldType != null) {
				if (FieldType != type) {
					ContainingType.Type.Compiler.Report.Error (7822, "All collection method accessors must accept identical element types.");
					return;
				}
				_fieldType = type;
			}
		}

	}

	public class TypeInfo 
	{
		public ClassOrStruct Type;

		public List<FieldInfo> Fields = new List<FieldInfo>();
		public Dictionary<string, FieldInfo> FieldsByName = new Dictionary<string, FieldInfo>();

		public TypeInfo(ClassOrStruct type)
		{
			Type = type;
		}

		public void AddFields()
		{
			var typeInfo = this;
			var predefAttrs = Type.Module.PredefinedAttributes;

			var memberVistor = new MemberVisitor ((member) => {

				if (!member.OptAttributes.Contains(predefAttrs.NonAssetAttribute)) {

					bool isPublic = (member.ModFlags & Modifiers.PUBLIC) != 0;
					bool isStatic = (member.ModFlags & Modifiers.STATIC) != 0;
					bool isExplicitAsset = member.OptAttributes.Contains(predefAttrs.AssetAttribute);

					if (!isStatic && (isPublic || isExplicitAsset)) {

						var field = member as Field;
						if (field != null) {
							typeInfo.AddFieldInfo(new FieldFieldInfo(typeInfo, field));
						}

						var prop = member as Property;
						if (prop != null) {
							typeInfo.AddFieldInfo(new PropertyFieldInfo(typeInfo, prop));
						}

						var method = member as Method;
						if (method != null && method.OptAttributes.Contains(predefAttrs.CollectionAccessorAttribute)) {
							var colAccessorAttr = method.OptAttributes.Search(predefAttrs.CollectionAccessorAttribute);
							string name = (string)((StringConstant)colAccessorAttr.GetNamedValue("Name")).GetValue();
							CollectionAccessorType accessorType = (CollectionAccessorType)(int)(((EnumConstant)colAccessorAttr.GetNamedValue("AccessorType")).GetValue());
							int elemParam = (int)((IntConstant)colAccessorAttr.GetNamedValue("ElementParam")).GetValue();
							CollectionFieldInfo colFieldInfo = null;
							FieldInfo fieldInfo = null;
							if (!FieldsByName.TryGetValue(name, out fieldInfo)) {
								colFieldInfo = new CollectionFieldInfo(typeInfo, name, elemParam);
								Fields.Add(colFieldInfo);
								FieldsByName[name] = colFieldInfo;
							} else {
								colFieldInfo = (CollectionFieldInfo)fieldInfo;
							}
							colFieldInfo.SetAccessorMethod(accessorType, method);
						}
					}
				}

				return true;
			});
			memberVistor.Visit (Type);
		}

		public void AddFieldInfo(FieldInfo fieldInfo) 
		{
			Fields.Add (fieldInfo);
			FieldsByName [fieldInfo.Name] = fieldInfo;
		}
	}

	public class RootTypeInfo : TypeInfo
	{

		public RootTypeInfo(ClassOrStruct type) : base(type) 
		{
		}

	}

	public class TypeVisitor : StructuralVisitor 
	{
		public Func<TypeDefinition, bool> Callback;

		public TypeVisitor(Func<TypeDefinition, bool> callback) {
			this.Callback = callback;
			this.AutoVisit = true;
			this.Depth = VisitDepth.Types;
		}

		public override void Visit (Mono.CSharp.Class cl)
		{
			if (!Callback (cl)) {
				this.Continue = false;
			} else {
				base.Visit (cl);
			}
		}

		public override void Visit (Mono.CSharp.Struct st)
		{
			if (!Callback (st)) {
				this.Continue = false;
			} else {
				base.Visit (st);
			}
		}
	}

	public class MemberVisitor : StructuralVisitor 
	{
		public Func<MemberCore, bool> Callback;

		public MemberVisitor(Func<MemberCore, bool> callback) {
			this.Callback = callback;
			this.AutoVisit = true;
			this.Depth = VisitDepth.Members;
		}

		public override void Visit (Mono.CSharp.Constructor c)
		{
			if (!Callback (c)) {
				this.Continue = false;
			} else {
				base.Visit (c);
			}
		}

		public override void Visit (Mono.CSharp.Method m)
		{
			if (!Callback (m)) {
				this.Continue = false;
			} else {
				base.Visit (m);
			}
		}

		public override void Visit (Mono.CSharp.Indexer i)
		{
			if (!Callback (i)) {
				this.Continue = false;
			} else {
				base.Visit (i);
			}
		}

		public override void Visit (Mono.CSharp.Operator o)
		{
			if (!Callback (o)) {
				this.Continue = false;
			} else {
				base.Visit (o);
			}
		}

		public override void Visit (Mono.CSharp.Property p)
		{
			if (!Callback (p)) {
				this.Continue = false;
			} else {
				base.Visit (p);
			}
		}

		public override void Visit (Mono.CSharp.Field f)
		{
			if (!Callback (f)) {
				this.Continue = false;
			} else {
				base.Visit (f);
			}
		}
	}

	public static class Tooling 
	{
		public static HashSet<ClassOrStruct> AssetClassesToAdd = new HashSet<ClassOrStruct>();
		public static HashSet<ClassOrStruct> RootAssetClassesToAdd = new HashSet<ClassOrStruct>();

		public static List<TypeInfo> AssetTypes = new List<TypeInfo>();
		public static Dictionary<ClassOrStruct, TypeInfo> AssetTypeInfosByTypeDef = new Dictionary<ClassOrStruct, TypeInfo> ();

		public static List<TypeInfo> RootAssetTypeInfos = new List<TypeInfo>();
		public static Dictionary<ClassOrStruct, TypeInfo> RootAssetTypeInfosByTypeDef = new Dictionary<ClassOrStruct, TypeInfo> ();

		public static void BuildAssetClassSet(ModuleContainer mc) {

			// Find all initial marked asset types
			var findTypes = new TypeVisitor ((type) => {
				var classOrStruct = type as ClassOrStruct;
				if (classOrStruct != null) {
					bool isAsset = type.OptAttributes.Contains(mc.PredefinedAttributes.AssetAttribute);
					bool isRootAsset = type.OptAttributes.Contains(mc.PredefinedAttributes.RootAssetAttribute);
					if (isAsset && !Tooling.AssetClassesToAdd.Contains(classOrStruct)) {
						Tooling.AssetClassesToAdd.Add(classOrStruct);
					}
					if (isRootAsset && !Tooling.RootAssetClassesToAdd.Contains(classOrStruct)) {
						Tooling.RootAssetClassesToAdd.Add(classOrStruct);
					}
				}
				return true;
			});
			findTypes.Visit(mc);

			// Recursively add until there is no more to add
			while (AssetClassesToAdd.Count > 0 && RootAssetClassesToAdd.Count > 0) {

				var assetClassesToAdd = AssetClassesToAdd;
				AssetClassesToAdd = new HashSet<ClassOrStruct> ();

				var rootAssetClassesToAdd = RootAssetClassesToAdd;
				RootAssetClassesToAdd = new HashSet<ClassOrStruct> ();

				foreach (var rootAssetType in rootAssetClassesToAdd) {
					var rootTypeInfo = new RootTypeInfo (rootAssetType);
					AssetTypes.Add (rootTypeInfo);
					AssetTypeInfosByTypeDef.Add (rootAssetType, rootTypeInfo);
					RootAssetTypeInfos.Add (rootTypeInfo);
					RootAssetTypeInfosByTypeDef.Add (rootAssetType, rootTypeInfo);
				}

				foreach (var assetType in assetClassesToAdd) {
					var typeInfo = new TypeInfo (assetType);
					AssetTypes.Add (typeInfo);
					AssetTypeInfosByTypeDef.Add (assetType, typeInfo);
				}

			}

		}

		private static FullNamedExpression NewEditContextTypeExpr()
		{
			return new MemberAccess (new MemberAccess (new MemberAccess (new SimpleName ("PlayScript", Location.Null), "Tooling", 
			                                                             Location.Null), "Editor", Location.Null), "EditContext", Location.Null);
		}

		private static string GetTypeName(TypeSpec type)
		{
			switch (type.BuiltinType) {
			case BuiltinTypeSpec.Type.Bool:
				return "bool";
			case BuiltinTypeSpec.Type.Char:
				return "char";
			case BuiltinTypeSpec.Type.Byte:
				return "byte";
			case BuiltinTypeSpec.Type.SByte:
				return "sbyte";
			case BuiltinTypeSpec.Type.Short:
				return "short";
			case BuiltinTypeSpec.Type.UShort:
				return "ushort";
			case BuiltinTypeSpec.Type.Int:
				return "int";
			case BuiltinTypeSpec.Type.UInt:
				return "uint";
			case BuiltinTypeSpec.Type.Long:
				return "long";
			case BuiltinTypeSpec.Type.ULong:
				return "ulong";
			case BuiltinTypeSpec.Type.Float:
				return "float";
			case BuiltinTypeSpec.Type.Double:
				return "double";
			case BuiltinTypeSpec.Type.String:
				return "string";
			default:
				return null;
			}
		}

		public static void ConvertClasses(ModuleContainer mc)
		{
			int i;

			var os = new StringWriter();

			os.Write (@"
// Generated tooling class partial classes

");

			foreach (var type in AssetTypes) {

				//
				// Convert fields to properties.
				//

				foreach (var field in type.Fields) {
					if (field is FieldFieldInfo) {
						if (field is FieldFieldInfo) {
							type.Type.RenameMember ("_" + field.Name);
							field.BackingField.ModFlags = (field.BackingField.ModFlags & ~(Modifiers.PUBLIC | Modifiers.PROTECTED)) | Modifiers.PRIVATE;
						}
					}
				}


					os.Write (@"
	namespace {1} {{

		partial class {2} : PlayScript.Tooling.Editor.IEditable {{

			private IntPtr __data;

			IntPtr PlayScript.Tooling.IAssetObject.Data {{
				get {{
					return __data;
				}}
			}}
", asdf);

			if (mc.Compiler.Settings.Tooling == ToolingMode.Editor) {
				os.Write (@"

			private uint __uid;
			private PlayScript.Tooling.AssetPool __assetPool;
			private PlayScript.Tooling.Editor.EditContext __editContext;
			private PlayScript.Tooling.Editor.IEditable __assetParent;
			private PlayScript.Tooling.Editor.IEditable __assetRoot;
			private PlayScript.Tooling.AssetPool _assetPool;

			PlayScript.Tooling.Editor.EditContext PlayScript.Tooling.Editor.IEditable.EditContext {{
				get {{
					return __editContext;
				}}
			}}

			PlayScript.Tooling.Editor.IEditable PlayScript.Tooling.Editor.IEditable.AssetParent {{
				get {{
					return __assetParent;
				}}
			}}
		
			PlayScript.Tooling.Editor.IEditable PlayScript.Tooling.Editor.IEditable.AssetRoot {{
				get {{
					return __assetRoot;
				}}
			}}

			bool PlayScript.Tooling.Editor.IEditable.IsSelected {{
				get {{
					return __editContext.IsSelected(this);
				}}
			}}

			FieldInfo[] PlayScript.Tooling.Editor.IEditable.FieldInfo {{
				get {{
					return __fieldInfo;
				}}
			}}

			int PlayScript.Tooling.Editor.IEditable.GetFieldIndex(string fieldName) {{
				int fieldId = -1;
				__fieldIdByName.TryGetValue(fieldName, out fieldId);
				return fieldId;
			}}
");
					FieldInfo fi = null;

					//
					// Write Field Wrappers
					//

				for (i = 0; i < type.Fields.Count; i++) {
					fi = type.Fields [i];

					if (fi is FieldFieldInfo || fi is PropertyFieldInfo) {
						string fieldTypeName = GetTypeName (fi.FieldType);
						if (fi.FieldType.IsClass || fi.FieldType.BuiltinType == BuiltinTypeSpec.Type.String) {
								os.Write (
@"						public {0} {1} {{
							get {{
								return {2};
							}}
							set {{
								if (__editContext.EditMode) {{
									__editContext.SetValue(this, {3}, new Value(value));
								}} else {{
									this.{2} = value;
								}}
							}}
						}}
					", fi.Name);
						} else {
							os.Write (
@"						value._{0} = {1};
					value.object = typeof({2});
					break;
					", fieldTypeName, fi.Name, fieldTypeName);
						}
					}

					if (fi is CollectionFieldInfo) {
						CollectionFieldInfo cfi = (CollectionFieldInfo)fi;
						string elemTypeName = GetTypeName (cfi.ElementType);
						if (fi.FieldType.IsClass || fi.FieldType.BuiltinType == BuiltinTypeSpec.Type.String) {
							os.Write (
@"						value._long = 0;
					value.object = {0}(index);
					break;
					", cfi.GetMethod.ShortName);
						} else {
							os.Write (
@"						value._{0} = {1}(index);
					value.object = typeof({2});
					break;
					", elemTypeName, cfi.GetMethod.ShortName, elemTypeName);
						}
					}
				}

					os.Write (@"
					}}
					}}
					");


					//
					// Write GetValue()
					//

					os.Write (@"
			void PlayScript.Tooling.Editor.IEditable.GetValue(int fieldId, ref Value value, int index = -1) {{

				switch (fieldId) {{
");

				for (i = 0; i < type.Fields.Count; i++) {
					fi = type.Fields [i];

					os.Write (@"
					case {0}:
", i);

					if (fi is FieldFieldInfo || fi is PropertyFieldInfo) {
						string fieldTypeName = GetTypeName (fi.FieldType);
						if (fi.FieldType.IsClass || fi.FieldType.BuiltinType == BuiltinTypeSpec.Type.String) {
								os.Write (
@"						value._long = 0;
						value.object = {0};
						break;
", fi.Name);
						} else {
							os.Write (
@"						value._{0} = {1};
						value.object = typeof({2});
						break;
", fieldTypeName, fi.Name, fieldTypeName);
						}
					}

					if (fi is CollectionFieldInfo) {
						CollectionFieldInfo cfi = (CollectionFieldInfo)fi;
						string elemTypeName = GetTypeName (cfi.ElementType);
						if (fi.FieldType.IsClass || fi.FieldType.BuiltinType == BuiltinTypeSpec.Type.String) {
							os.Write (
@"						value._long = 0;
						value.object = {0}(index);
						break;
", cfi.GetMethod.ShortName);
						} else {
							os.Write (
@"						value._{0} = {1}(index);
						value.object = typeof({2});
						break;
", elemTypeName, cfi.GetMethod.ShortName, elemTypeName);
						}
					}
				}

				os.Write (@"
				}}
			}}
");

					//
					// Write SetValue()
					//

					os.Write (@"
			void PlayScript.Tooling.Editor.IEditable.SetValue(int fieldId, ref Value value, int index = -1) {{

				switch (fieldId) {{
");

				for (i = 0; i < type.Fields.Count; i++) {
					fi = type.Fields [i];

					os.Write (@"
					case {0}:
", i);

					if (fi is FieldFieldInfo || fi is PropertyFieldInfo) {
						string fieldTypeName = GetTypeName (fi.FieldType);
						if (fi.FieldType.IsClass || fi.FieldType.BuiltinType == BuiltinTypeSpec.Type.String) {
								os.Write (
@"						this.{0} = value.object;
						if (__editContext.EditMode) {{
							PlayScript.Tooling.Editor.EditorHelpers.SetObjectValue((byte*)(__data.ToPointer()) + {1}, value.object);
						}}}
						break;
", fi.Name);
						} else {
							os.Write (
@"						this.{0} = value._{1};
						if (__editContext.EditMode) {{
							*({1})((byte*)(__data.ToPointer()) + {1}) = value._{1};
						}}}
						break;
", fi.Name, fieldTypeName);
						}
					}

					if (fi is CollectionFieldInfo) {
						CollectionFieldInfo cfi = (CollectionFieldInfo)fi;
						string elemTypeName = GetTypeName (cfi.ElementType);
						if (fi.FieldType.IsClass || fi.FieldType.BuiltinType == BuiltinTypeSpec.Type.String) {
							os.Write (
@"						value._long = 0;
						value.object = {0}[index];
						break;
", fi.BackingField.Name);
						} else {
							os.Write (
@"						value._{0} = {1}[index];
								value.object = typeof({2});
								break;
								", elemTypeName, fi.BackingField.Name, elemTypeName);
						}
					}
				}

					os.Write (@"
				}}
			}}
");


				os.Write (@"
		}}
	}}
");
			string fileStr = os.ToString();
			var path = System.IO.Path.Combine (System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(module.Compiler.Settings.OutputFile)), "dynamic.g.cs");
			System.IO.File.WriteAllText(path, fileStr);

			byte[] byteArray = Encoding.ASCII.GetBytes( fileStr );
			var input = new MemoryStream( byteArray, false );
			var reader = new SeekableStreamReader (input, System.Text.Encoding.UTF8);

			SourceFile file = new SourceFile(path, path, 0);
			file.FileType = SourceFileType.CSharp;

			Driver.Parse (reader, file, module, session, report);

		}


		}

	}

	}


}


