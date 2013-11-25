package
{
	public class ConversionTest
	{
		public static function Main():int
		{
			// return testRun(doTests);
			return testRun(doTests);

		}

		public const constInt:int = 1234;

		public const constNumber:Number = Number(constInt / 2);
		
		public static function doTests():void
		{
			var nanValue:Number = NaN;
			var u:* = undefined;
			var nullObj:* = null;
			var strObj:Object = "1024";
			var obj:Object = new Object();
			var i:int;
			var ui:uint;
			var n:Number;
			var b:Boolean;

			// 'as' allows conversions as long as the value can be fully represented in the destination type
			obj = 5;
			test((obj as Number) == 5.0);
			obj = 2.0;
			i = (obj as int);
			test(i == 2);
			obj = 5.4;
			i = (obj as int); 
			test(i == 0);

			// 'as' allows conversions as long as the value can be fully represented in the destination type
			i = 5;
			ui = i as uint;
			test(ui == 5);
			i = -5;
			ui = i as uint;
			test(ui == 0);

			// 'as' doesnt cast from boolean to number
			b = true;
			n = (b as Number); 
			test(n == 0.0);
			b = false;
			n = (b as Number);
			test(n == 0.0);

			// explicit cast does cast from boolean to number
			b = true;
			n = Number(b); 
			test(n == 1.0);
			b = false;
			n = Number(b);
			test(n == 0.0);
			
			// 'as' doesnt cast from boolean to int
			b = true;
			i = (b as int); 
			test(i == 0);
			b = false;
			i = (b as int);
			test(i == 0);

			// explicit cast does cast from boolean to int
			b = true;
			i = int(b); 
			test(i == 1);
			b = false;
			i = int(b);
			test(i == 0);

			test(Number(true) == 1.0);
			test(int(true) == 1);
			
			test(int(strObj) == 1024);
			n = Number(strObj);
			test(n == 1024.0);
			i = (strObj as int);
			test(i == 0);   // <<= broken
			n = (strObj as Number);
			test(n == 0.0); // <<= broken
			
			test(int("0x1234") == 4660);
			test(int("0X1234") == 4660); // <<= broken
			test(int("0xFFFFFFFF") == -1); // (or 0xFFFFFFFF)  // <<= broken
			// test(int("0xFFFFFFFF4444") == 0xFFFF4444);  // <<= broken
			test(Number("0x1234") == 4660.0); // <<= broken
			
			// invalid parsing (0 for int, NaN for number)
			test(int("xyz") == 0);  // <<= broken (this throws)
			test(isNaN(Number("xyz")));  // <<= broken (this throws)
			
			// number conversion of undefined (NaN for explicit cast, 0.0 for 'as' cast)
			test(isNaN(Number(u)));               
			n = (u as Number);
			test(n == 0.0);  // <<= broken
			
			// int conversion of undefined (0 for explicit cast, 0 for 'as' cast)
			test(int(u) == 0);
			i = (u as int);
			test(i == 0);
			
			// null to number produces 0.0
			test(Number(nullObj) == 0.0);
			n = nullObj as Number;
			test(n == 0.0);
			
			// int to number does proper cast
			obj = 5;
			n = Number(obj);
			test(n == 5.0);
			n = (obj as Number);
			test(n == 5.0); // <<= broken 
			
			// conversion of NaN to int
			i = int(nanValue);
//			test(i == 0);
			i = (nanValue as int);
			test(i == 0);
			
			// casting of undefined or null produces the string "undefined" or "null", but not with as
			test(String(u) == "undefined");     // <<= sometimes works but not always       
			test(String(null) == "null");               // <<= sometimes works but not always       
			test((u as String) == null);
			test((nullObj as String) == null);
			
			// casting of object to String invokes toString() on object, but not with as
			obj = new Object();
			test(String(obj) == obj.toString());
			test((obj as String) == null);
			
			// object as number produces 0.0, Number(object) produces NaN
			n = Number(obj);
			test(isNaN(n));
			n = obj as Number;
			test(n == 0.0); // <<= broken 
			i = obj as int;
			test(i == 0);
			
			// conversion of numerics to boolean 
			b = Boolean(0);
			test(b == false);
			b = Boolean(1);
			test(b == true);
			b = Boolean(0.0);
			test(b == false);
			b = Boolean(1.0);
			test(b == true);

			// conversion of strings to boolean (we had this wrong, only "" is false)
			b = Boolean("");
			test(b == false);
			b = Boolean("0");
			test(b == true);
			b = Boolean("1");
			test(b == true);
			b = Boolean("false");
			test(b == true);
			b = Boolean("true");
			test(b == true);
			b = Boolean("FALSE");
			test(b == true);
			b = Boolean("TRUE");
			test(b == true);
			
			b = ("" as Boolean);
			test(b == false);
			b = ("0" as Boolean);
			test(b == false);
			b = ("1" as Boolean);
			test(b == false);
			b = ("false" as Boolean);
			test(b == false);
			b = ("true" as Boolean);
			test(b == false);
			b = ("FALSE" as Boolean);
			test(b == false);
			b = ("TRUE" as Boolean);
			test(b == false);
		}
	}
}
