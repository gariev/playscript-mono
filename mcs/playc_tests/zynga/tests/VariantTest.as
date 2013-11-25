package
{
	import PlayScript.Variant;

	public class VariantTest
	{
		public static function Main():int
		{
			return testRun(doTests);
		}

		
		public static function doTests():void
		{
			// setup local values
			var d0:Boolean = false;
			var d1:Boolean = true;
			var d2:int = 0;
			var d3:int = -1234;
			var d4:int = 0u;
			var d5:int = 1234u;
			var d6:Number = 0.0;
			var d7:Number = 5.23;
			var d8:String = "";
			var d9:String = "abc";
			var dA:Object = new Object();
			var dB:Object = new VariantTest();

			// assign values into variants, this should use implicit casts provided by Variant
			var v0:Variant = d0;
			var v1:Variant = d1;
			var v2:Variant = d2;
			var v3:Variant = d3;
			var v4:Variant = d4;
			var v5:Variant = d5;
			var v6:Variant = d6;
			var v7:Variant = d7;
			var v8:Variant = d8;
			var v9:Variant = d9;
			var vA:Variant = dA;
			var vB:Variant = dB;

			// do testing of all explicit conversions

			test(Boolean(d0) == Boolean(v0));
			test(Boolean(d1) == Boolean(v1));
			test(Boolean(d2) == Boolean(v2));
			test(Boolean(d3) == Boolean(v3));
			test(Boolean(d4) == Boolean(v4));
			test(Boolean(d5) == Boolean(v5));
			test(Boolean(d6) == Boolean(v6));
			test(Boolean(d7) == Boolean(v7));
			test(Boolean(d8) == Boolean(v8));
			test(Boolean(d9) == Boolean(v9));
			test(Boolean(dA) == Boolean(vA));
			test(Boolean(dB) == Boolean(vB));

			test(int(d0) == int(v0));
			test(int(d1) == int(v1));
			test(int(d2) == int(v2));
			test(int(d3) == int(v3));
			test(int(d4) == int(v4));
			test(int(d5) == int(v5));
			test(int(d6) == int(v6));
			test(int(d7) == int(v7));
			test(int(d8) == int(v8));
			test(int(d9) == int(v9));
			test(int(dA) == int(vA));
			test(int(dB) == int(vB));

			test(uint(d0) == uint(v0));
			test(uint(d1) == uint(v1));
			test(uint(d2) == uint(v2));
			test(uint(d3) == uint(v3));
			test(uint(d4) == uint(v4));
			test(uint(d5) == uint(v5));
			test(uint(d6) == uint(v6));
			test(uint(d7) == uint(v7));
			test(uint(d8) == uint(v8));
			test(uint(d9) == uint(v9));
			test(uint(dA) == uint(vA));
			test(uint(dB) == uint(vB));

			test(Number(d0) == Number(v0));
			test(Number(d1) == Number(v1));
			test(Number(d2) == Number(v2));
			test(Number(d3) == Number(v3));
			test(Number(d4) == Number(v4));
			test(Number(d5) == Number(v5));
			test(Number(d6) == Number(v6));
			test(Number(d7) == Number(v7));
			test(isNaN(Number(d8)) == isNaN(Number(v8)));
			test(isNaN(Number(d9)) == isNaN(Number(v9)));
			test(isNaN(Number(dA)) == isNaN(Number(vA)));
			test(isNaN(Number(dB)) == isNaN(Number(vB)));

			test(String(d0) == String(v0));
			test(String(d1) == String(v1));
			test(String(d2) == String(v2));
			test(String(d3) == String(v3));
			test(String(d4) == String(v4));
			test(String(d5) == String(v5));
			test(String(d6) == String(v6));
			test(String(d7) == String(v7));
			test(String(d8) == String(v8));
			test(String(d9) == String(v9));
			test(String(dA) == String(vA));
			test(String(dB) == String(vB));


			// do testing of all "as" conversions
			test((d0 as Boolean) == (v0 as Boolean));
			test((d1 as Boolean) == (v1 as Boolean));
			test((d2 as Boolean) == (v2 as Boolean));
			test((d3 as Boolean) == (v3 as Boolean));
			test((d4 as Boolean) == (v4 as Boolean));
			test((d5 as Boolean) == (v5 as Boolean));
			test((d6 as Boolean) == (v6 as Boolean));
			test((d7 as Boolean) == (v7 as Boolean));
			test((d8 as Boolean) == (v8 as Boolean));
			test((d9 as Boolean) == (v9 as Boolean));
			test((dA as Boolean) == (vA as Boolean));
			test((dB as Boolean) == (vB as Boolean));

			test((d0 as int) == (v0 as int));
			test((d1 as int) == (v1 as int));
			test((d2 as int) == (v2 as int));
			test((d3 as int) == (v3 as int));
			test((d4 as int) == (v4 as int));
			test((d5 as int) == (v5 as int));
			test((d6 as int) == (v6 as int));
			test((d7 as int) == (v7 as int));
			test((d8 as int) == (v8 as int));
			test((d9 as int) == (v9 as int));
			test((dA as int) == (vA as int));
			test((dB as int) == (vB as int));

			test((d0 as uint) == (v0 as uint));
			test((d1 as uint) == (v1 as uint));
			test((d2 as uint) == (v2 as uint));
			test((d3 as uint) == (v3 as uint));
			test((d4 as uint) == (v4 as uint));
			test((d5 as uint) == (v5 as uint));
			test((d6 as uint) == (v6 as uint));
			test((d7 as uint) == (v7 as uint));
			test((d8 as uint) == (v8 as uint));
			test((d9 as uint) == (v9 as uint));
			test((dA as uint) == (vA as uint));
			test((dB as uint) == (vB as uint));

			test((d0 as Number) == (v0 as Number));
			test((d1 as Number) == (v1 as Number));
			test((d2 as Number) == (v2 as Number));
			test((d3 as Number) == (v3 as Number));
			test((d4 as Number) == (v4 as Number));
			test((d5 as Number) == (v5 as Number));
			test((d6 as Number) == (v6 as Number));
			test((d7 as Number) == (v7 as Number));
			test((d8 as Number) == (v8 as Number));
			test((d9 as Number) == (v9 as Number));
			test((dA as Number) == (vA as Number));
			test((dB as Number) == (vB as Number));

			test((d0 as String) == (v0 as String));
			test((d1 as String) == (v1 as String));
			test((d2 as String) == (v2 as String));
			test((d3 as String) == (v3 as String));
			test((d4 as String) == (v4 as String));
			test((d5 as String) == (v5 as String));
			test((d6 as String) == (v6 as String));
			test((d7 as String) == (v7 as String));
			test((d8 as String) == (v8 as String));
			test((d9 as String) == (v9 as String));
			test((dA as String) == (vA as String));
			test((dB as String) == (vB as String));
		}
	}
}
