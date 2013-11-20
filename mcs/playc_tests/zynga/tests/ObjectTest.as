package
{
	import Amf.*;

	public class ObjectTest
	{
		public static function Main():int
		{
			return testRun(doTests);
		}


		public static function testObject(obj:Object):void
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
			var dB:Object = new ObjectTest();
			var dC:ObjectTest = new ObjectTest();

			// store all values in object
			obj.d0 = d0;
			obj.d1 = d1;
			obj.d2 = d2;
			obj.d3 = d3;
			obj.d4 = d4;
			obj.d5 = d5;
			obj.d6 = d6;
			obj.d7 = d7;
			obj.d8 = d8;
			obj.d9 = d9;
			obj.dA = dA;
			obj.dB = dB;
			obj.dC = dC;

			// read values out of object
			var e0:Boolean = obj.d0;
			var e1:Boolean = obj.d1;
			var e2:int  = obj.d2;
			var e3:int  = obj.d3;
			var e4:int  = obj.d4;
			var e5:int  = obj.d5;
			var e6:Number  = obj.d6;
			var e7:Number  = obj.d7;
			var e8:String  = obj.d8;
			var e9:String  = obj.d9;
			var eA:Object  = obj.dA;
			var eB:Object  = obj.dB;
			var eC:ObjectTest  = obj.dC;

			test(d0 == e0);
			test(d1 == e1);
			test(d2 == e2);
			test(d3 == e3);
			test(d4 == e4);
			test(d5 == e5);
			test(d6 == e6);
			test(d7 == e7);
			test(d8 == e8);
			test(d9 == e9);
			test(dA == eA);
			test(dB == eB);
			test(dC == eC);

			// read values out of object using indexer
			e0  = obj["d0"];
			e1  = obj["d1"];
			e2  = obj["d2"];
			e3  = obj["d3"];
			e4  = obj["d4"];
			e5  = obj["d5"];
			e6  = obj["d6"];
			e7  = obj["d7"];
			e8  = obj["d8"];
			e9  = obj["d9"];
			eA  = obj["dA"];
			eB  = obj["dB"];
			eC  = obj["dC"];

			test(d0 == e0);
			test(d1 == e1);
			test(d2 == e2);
			test(d3 == e3);
			test(d4 == e4);
			test(d5 == e5);
			test(d6 == e6);
			test(d7 == e7);
			test(d8 == e8);
			test(d9 == e9);
			test(dA == eA);
			test(dB == eB);
			test(dC == eC);

			test(Boolean(d0) == Boolean(obj.d0));
			test(Boolean(d1) == Boolean(obj.d1));
			test(Boolean(d2) == Boolean(obj.d2));
			test(Boolean(d3) == Boolean(obj.d3));
			test(Boolean(d4) == Boolean(obj.d4));
			test(Boolean(d5) == Boolean(obj.d5));
			test(Boolean(d6) == Boolean(obj.d6));
			test(Boolean(d7) == Boolean(obj.d7));
			test(Boolean(d8) == Boolean(obj.d8));
			test(Boolean(d9) == Boolean(obj.d9));
			test(Boolean(dA) == Boolean(obj.dA));
			test(Boolean(dB) == Boolean(obj.dB));

			test(int(d0) == int(obj.d0));
			test(int(d1) == int(obj.d1));
			test(int(d2) == int(obj.d2));
			test(int(d3) == int(obj.d3));
			test(int(d4) == int(obj.d4));
			test(int(d5) == int(obj.d5));
			test(int(d6) == int(obj.d6));
			test(int(d7) == int(obj.d7));
			test(int(d8) == int(obj.d8));
			test(int(d9) == int(obj.d9));
			test(int(dA) == int(obj.dA));
			test(int(dB) == int(obj.dB));

			test(uint(d0) == uint(obj.d0));
			test(uint(d1) == uint(obj.d1));
			test(uint(d2) == uint(obj.d2));
			test(uint(d3) == uint(obj.d3));
			test(uint(d4) == uint(obj.d4));
			test(uint(d5) == uint(obj.d5));
			test(uint(d6) == uint(obj.d6));
			test(uint(d7) == uint(obj.d7));
			test(uint(d8) == uint(obj.d8));
			test(uint(d9) == uint(obj.d9));
			test(uint(dA) == uint(obj.dA));
			test(uint(dB) == uint(obj.dB));

			test(Number(d0) == Number(obj.d0));
			test(Number(d1) == Number(obj.d1));
			test(Number(d2) == Number(obj.d2));
			test(Number(d3) == Number(obj.d3));
			test(Number(d4) == Number(obj.d4));
			test(Number(d5) == Number(obj.d5));
			test(Number(d6) == Number(obj.d6));
			test(Number(d7) == Number(obj.d7));
			test(isNaN(Number(d8)) == isNaN(Number(obj.d8)));
			test(isNaN(Number(d9)) == isNaN(Number(obj.d9)));
			test(isNaN(Number(dA)) == isNaN(Number(obj.dA)));
			test(isNaN(Number(dB)) == isNaN(Number(obj.dB)));

			test(String(d0) == String(obj.d0));
			test(String(d1) == String(obj.d1));
			test(String(d2) == String(obj.d2));
			test(String(d3) == String(obj.d3));
			test(String(d4) == String(obj.d4));
			test(String(d5) == String(obj.d5));
			test(String(d6) == String(obj.d6));
			test(String(d7) == String(obj.d7));
			test(String(d8) == String(obj.d8));
			test(String(d9) == String(obj.d9));
			test(String(dA) == String(obj.dA));
			test(String(dB) == String(obj.dB));


			// do testing of all "as" conversions
			test((d0 as Boolean) == (obj.d0 as Boolean));
			test((d1 as Boolean) == (obj.d1 as Boolean));
			test((d2 as Boolean) == (obj.d2 as Boolean));
			test((d3 as Boolean) == (obj.d3 as Boolean));
			test((d4 as Boolean) == (obj.d4 as Boolean));
			test((d5 as Boolean) == (obj.d5 as Boolean));
			test((d6 as Boolean) == (obj.d6 as Boolean));
			test((d7 as Boolean) == (obj.d7 as Boolean));
			test((d8 as Boolean) == (obj.d8 as Boolean));
			test((d9 as Boolean) == (obj.d9 as Boolean));
			test((dA as Boolean) == (obj.dA as Boolean));
			test((dB as Boolean) == (obj.dB as Boolean));

			test((d0 as int) == (obj.d0 as int));
			test((d1 as int) == (obj.d1 as int));
			test((d2 as int) == (obj.d2 as int));
			test((d3 as int) == (obj.d3 as int));
			test((d4 as int) == (obj.d4 as int));
			test((d5 as int) == (obj.d5 as int));
			test((d6 as int) == (obj.d6 as int));
			test((d7 as int) == (obj.d7 as int));
			test((d8 as int) == (obj.d8 as int));
			test((d9 as int) == (obj.d9 as int));
			test((dA as int) == (obj.dA as int));
			test((dB as int) == (obj.dB as int));

			test((d0 as uint) == (obj.d0 as uint));
			test((d1 as uint) == (obj.d1 as uint));
			test((d2 as uint) == (obj.d2 as uint));
			test((d3 as uint) == (obj.d3 as uint));
			test((d4 as uint) == (obj.d4 as uint));
			test((d5 as uint) == (obj.d5 as uint));
			test((d6 as uint) == (obj.d6 as uint));
			test((d7 as uint) == (obj.d7 as uint));
			test((d8 as uint) == (obj.d8 as uint));
			test((d9 as uint) == (obj.d9 as uint));
			test((dA as uint) == (obj.dA as uint));
			test((dB as uint) == (obj.dB as uint));

			test((d0 as Number) == (obj.d0 as Number));
			test((d1 as Number) == (obj.d1 as Number));
			test((d2 as Number) == (obj.d2 as Number));
			test((d3 as Number) == (obj.d3 as Number));
			test((d4 as Number) == (obj.d4 as Number));
			test((d5 as Number) == (obj.d5 as Number));
			test((d6 as Number) == (obj.d6 as Number));
			test((d7 as Number) == (obj.d7 as Number));
			test((d8 as Number) == (obj.d8 as Number));
			test((d9 as Number) == (obj.d9 as Number));
			test((dA as Number) == (obj.dA as Number));
			test((dB as Number) == (obj.dB as Number));

			test((d0 as String) == (obj.d0 as String));
			test((d1 as String) == (obj.d1 as String));
			test((d2 as String) == (obj.d2 as String));
			test((d3 as String) == (obj.d3 as String));
			test((d4 as String) == (obj.d4 as String));
			test((d5 as String) == (obj.d5 as String));
			test((d6 as String) == (obj.d6 as String));
			test((d7 as String) == (obj.d7 as String));
			test((d8 as String) == (obj.d8 as String));
			test((d9 as String) == (obj.d9 as String));
			test((dA as String) == (obj.dA as String));
			test((dB as String) == (obj.dB as String));
		}

		
		public static function doTests():void
		{
			// expando object
			var obj:Object = new Object();
			testObject(obj);

			// amf object
			var amfObj:Object = new Amf.Amf3Object(Amf3ClassDef.Anonymous);
			testObject(amfObj);
		}
	}
}
