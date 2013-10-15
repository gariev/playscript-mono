using System;
using System.Collections.Generic;
using Mono.CSharp;

namespace Mono.PlayScript.Tooling
{

	public class FieldInfo
	{
		public TypeInfo ContainingType;
		public Field BackingField;
		public TypeSpec FieldType;
		public TypeInfo FieldTypeInfo;
		public string Name;
		public string Help;
		public string Category;
		public int Start;
		public int Length;

		public virtual MemberCore Member {
			get { return null; }
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
			FieldType = field.TypeExpression.ResolveAsType(field.Parent);
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
			FieldType = prop.TypeExpression.ResolveAsType(prop.Parent);
			if (prop.Get == null) {
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
		/// <summary>Method which removes an element from the collection.</summary>
		Remove,
		/// <summary>Method which gets an element from the collection.</summary>
		Get,
		/// <summary>Method which sets an element in the collection.</summary>
		Set,
		/// <summary>Method which inserts an element into the collection.</summary>
		Insert,
		/// <summary>Method which returns the index of an element in the collection.</summary>
		IndexOf,
		/// <summary>Method which returns true if an element is contained in the collection.</summary>
		Contains,
		/// <summary>Method which is used to initialize a new collection with a vector of initial items.</summary>
		Initialize
	}

	public class CollectionFieldInfo : FieldInfo
	{
		public Method AddMethod;
		public Method RemoveMethod;
		public Method GetMethod;
		public Method SetMethod;
		public Method InsertMethod;
		public Method IndexOfMethod;
		public Method ContainsMethod;
		public Method InitializeMethod;

		public CollectionFieldInfo(TypeInfo containingType, string name)
		{
			ContainingType = containingType;
			Name = name;
		}

		public void SetAccessorMethod(CollectionAccessorType accessorType, Method method)
		{
			switch (accessorType) {
			case CollectionAccessorType.Add:
				AddMethod = method;
				if (AddMethod.ParameterInfo.Types.Length > 0) {
					SetType (((Parameter)AddMethod.ParameterInfo[0]).TypeExpression.ResolveAsType(AddMethod.Parent));
				}
				break;
			case CollectionAccessorType.Remove:
				RemoveMethod = method;
				break;
			case CollectionAccessorType.Get:
				GetMethod = method;
				break;
			case CollectionAccessorType.Set:
				SetMethod = method;
				break;
			case CollectionAccessorType.Insert:
				InsertMethod = method;
				break;
			case CollectionAccessorType.IndexOf:
				IndexOfMethod = method;
				break;
			case CollectionAccessorType.Contains:
				ContainsMethod = method;
				break;
			case CollectionAccessorType.Initialize:
				InitializeMethod = method;
				break;
			}
		}

		public void SetType(TypeSpec type)
		{
			if (FieldType != null) {
				if (FieldType != type) {
					ContainingType.Type.Compiler.Report.Error (7822, "All collection method accessors must accept identical element types.");
					return;
				}
				FieldType = type;
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
							CollectionFieldInfo colFieldInfo = null;
							FieldInfo fieldInfo = null;
							if (!FieldsByName.TryGetValue(name, out fieldInfo)) {
								colFieldInfo = new CollectionFieldInfo(typeInfo, name);
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

		public static List<TypeInfo> AssetTypeInfos = new List<TypeInfo>();
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
					AssetTypeInfos.Add (rootTypeInfo);
					AssetTypeInfosByTypeDef.Add (rootAssetType, rootTypeInfo);
					RootAssetTypeInfos.Add (rootTypeInfo);
					RootAssetTypeInfosByTypeDef.Add (rootAssetType, rootTypeInfo);
				}

				foreach (var assetType in assetClassesToAdd) {
					var typeInfo = new TypeInfo (assetType);
					AssetTypeInfos.Add (typeInfo);
					AssetTypeInfosByTypeDef.Add (assetType, typeInfo);
				}

			}

		}


	}


}

