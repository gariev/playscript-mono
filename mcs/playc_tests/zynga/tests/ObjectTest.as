package
{
	import Amf.*;

	public class ObjectTest
	{
		public static function Main():int
		{
			return testRun(doTests);
		}


		public static function testObjectMember(obj:Object):void
		{
			// setup local values
			var d0:Boolean = false;
			var d1:Boolean = true;
			var d2:int = 0;
			var d3:int = -1234;
			var d4:uint = 0u;
			var d5:uint = 1234u;
			var d6:Number = 0.0;
			var d7:Number = 5.23;
			var d8:String = "";
			var d9:String = "abc";
			var dA:Object = new Object();
			var dB:Object = new ObjectTest();
			var dC:ObjectTest = new ObjectTest();
			var dD:* = null;
			var dE:* = null;
			var dF:* = undefined;
			var dG:Number = NaN;
			var dH:String = "0xFEFF";
			var dI:String = "2.5";

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
			obj.dD = dD;
			obj.dE = dE;
			obj.dF = dF;
			obj.dG = dG;
			obj.dH = dH;
			obj.dI = dI;
			obj.dnull = null;

			// read missing values with explicit casts
			test(Boolean(obj.missing) == false);
			test(int(obj.missing) == 0);
			test(uint(obj.missing) == 0);
			test(isNaN(Number(obj.missing)));
			test(String(obj.missing) == "undefined");
			test(Object(obj.missing) == null);

			// read null values with explicit casts
			test(Boolean(obj.dnull) == false);
			test(int(obj.dnull) == 0);
			test(uint(obj.dnull) == 0);
			test(Number(obj.dnull) == 0.0);
			test(String(obj.dnull) == "null");
			test(Object(obj.dnull) == null);

			// read missing values with as casts
			test((obj.missing as Boolean) == false);
			test((obj.missing as int) == 0);
			test((obj.missing as uint) == 0);
			test((obj.missing as Number) == 0.0);
			test((obj.missing as String) == null);
			test((obj.missing as Object) == null);

			// read null values with as casts
			test((obj.dnull as Boolean) == false);
			test((obj.dnull as int) == 0);
			test((obj.dnull as uint) == 0);
			test((obj.dnull as Number) == 0.0);
			test((obj.dnull as String) == null);
			test((obj.dnull as Object) == null);

			var str:String = obj.dD;
			test(str == null);
			str = obj.dF;
			test(str == null);


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
			var eD:*  = obj.dD;
			var eE:*  = obj.dE;
			var eF:*  = obj.dF;
			var eG:Number = obj.dG;
			var eH:String = obj.dH;
			var eI:String = obj.dI;

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
			test(dD == eD);
			test(dE == eE);
			test(dF == eF);
			test(isNaN(dG) && isNaN(eG));
			test(dH == eH);
			test(dI == eI);

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
			test(Boolean(dC) == Boolean(obj.dC));
			test(Boolean(dD) == Boolean(obj.dD));
			test(Boolean(dE) == Boolean(obj.dE));
			test(Boolean(dF) == Boolean(obj.dF));
			test(Boolean(dG) == Boolean(obj.dG));
			test(Boolean(dH) == Boolean(obj.dH));
			test(Boolean(dI) == Boolean(obj.dI));

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
			test(0 == int(obj.dC));
			test(int(dD) == int(obj.dD));
			test(int(dE) == int(obj.dE));
			test(int(dF) == int(obj.dF));
			test(int(dG) == int(obj.dG));
			test(int(dH) == int(obj.dH));
			test(int(dI) == int(obj.dI));

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
			test(0 == uint(obj.dC));
			test(uint(dD) == uint(obj.dD));
			test(uint(dE) == uint(obj.dE));
			test(uint(dF) == uint(obj.dF));
			test(uint(dG) == uint(obj.dG));
			test(uint(dH) == uint(obj.dH));
			test(uint(dI) == uint(obj.dI));


			test(Number(d0) == Number(obj.d0));
			test(Number(d1) == Number(obj.d1));
			test(Number(d2) == Number(obj.d2));
			test(Number(d3) == Number(obj.d3));
			test(Number(d4) == Number(obj.d4));
			test(Number(d5) == Number(obj.d5));
			test(Number(d6) == Number(obj.d6));
			test(Number(d7) == Number(obj.d7));
			test(isNaN(Number(d8)) && isNaN(Number(obj.d8)));
			test(isNaN(Number(d9)) && isNaN(Number(obj.d9)));
			test(isNaN(Number(dA)) && isNaN(Number(obj.dA)));
			test(isNaN(Number(dB)) && isNaN(Number(obj.dB)));
			test(isNaN(Number(obj.dC)));
			test(Number(dD) == Number(obj.dD));
			test(Number(dE) == Number(obj.dE));
			test(isNaN(Number(dF)) && isNaN(Number(obj.dF)));
			test(isNaN(Number(dG)) && isNaN(Number(obj.dG)));
			test(Number(dH) == Number(obj.dH));
			test(Number(dI) == Number(obj.dI));


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
			test(String(dC) == String(obj.dC));
			test(String(dD) == String(obj.dD));
			test(String(dE) == String(obj.dE));
			test(String(dF) == String(obj.dF));
			test(String(dG) == String(obj.dG));
			test(String(dH) == String(obj.dH));
			test(String(dI) == String(obj.dI));


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
			test((dC as Boolean) == (obj.dC as Boolean));
			test((dD as Boolean) == (obj.dD as Boolean));
			test((dE as Boolean) == (obj.dE as Boolean));
			test((dF as Boolean) == (obj.dF as Boolean));
			test((dG as Boolean) == (obj.dG as Boolean));
			test((dH as Boolean) == (obj.dH as Boolean));
			test((dI as Boolean) == (obj.dI as Boolean));

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
			test((dC as int) == (obj.dC as int));
			test((dD as int) == (obj.dD as int));
			test((dE as int) == (obj.dE as int));
			test((dF as int) == (obj.dF as int));
			test((dG as int) == (obj.dG as int));
			test((dH as int) == (obj.dH as int));
			test((dI as int) == (obj.dI as int));

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
			test((dC as uint) == (obj.dC as uint));
			test((dD as uint) == (obj.dD as uint));
			test((dE as uint) == (obj.dE as uint));
			test((dF as uint) == (obj.dF as uint));
			test((dG as uint) == (obj.dG as uint));
			test((dH as uint) == (obj.dH as uint));
			test((dI as uint) == (obj.dI as uint));

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
			test((dC as Number) == (obj.dC as Number));
			test((dD as Number) == (obj.dD as Number));
			test((dE as Number) == (obj.dE as Number));
			test((dF as Number) == (obj.dF as Number));
			test(isNaN((dG as Number)) && isNaN((obj.dG as Number)));
			test((dH as Number) == (obj.dH as Number));
			test((dI as Number) == (obj.dI as Number));

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
			test((dC as String) == (obj.dC as String));
			test((dD as String) == (obj.dD as String));
			test((dE as String) == (obj.dE as String));
			test((dF as String) == (obj.dF as String));
			test((dG as String) == (obj.dG as String));
			test((dH as String) == (obj.dH as String));
			test((dI as String) == (obj.dI as String));
		}


		public static function testObjectIndex(obj:Object):void
		{
			// setup local values
			var d0:Boolean = false;
			var d1:Boolean = true;
			var d2:int = 0;
			var d3:int = -1234;
			var d4:uint = 0u;
			var d5:uint = 1234u;
			var d6:Number = 0.0;
			var d7:Number = 5.23;
			var d8:String = "";
			var d9:String = "abc";
			var dA:Object = new Object();
			var dB:Object = new ObjectTest();
			var dC:ObjectTest = new ObjectTest();
			var dD:* = null;
			var dE:* = null;
			var dF:* = undefined;
			var dG:Number = NaN;
			var dH:String = "0xFEFF";
			var dI:String = "2.5";

			// store all values in object
			obj["d0"] = d0;
			obj["d1"] = d1;
			obj["d2"] = d2;
			obj["d3"] = d3;
			obj["d4"] = d4;
			obj["d5"] = d5;
			obj["d6"] = d6;
			obj["d7"] = d7;
			obj["d8"] = d8;
			obj["d9"] = d9;
			obj["dA"] = dA;
			obj["dB"] = dB;
			obj["dC"] = dC;
			obj["dD"] = dD;
			obj["dE"] = dE;
			obj["dF"] = dF;
			obj["dG"] = dG;
			obj["dH"] = dH;
			obj["dI"] = dI;
			obj["dnull"] = null;

			// read missing values with explicit casts
			test(Boolean(obj["missing"]) == false);
			test(int(obj["missing"]) == 0);
			test(uint(obj["missing"]) == 0);
			test(isNaN(Number(obj["missing"])));
			test(String(obj["missing"]) == "undefined");
			test(Object(obj["missing"]) == null);

			// read null values with explicit casts
			test(Boolean(obj["dnull"]) == false);
			test(int(obj["dnull"]) == 0);
			test(uint(obj["dnull"]) == 0);
			test(Number(obj["dnull"]) == 0.0);
			test(String(obj["dnull"]) == "null");
			test(Object(obj["dnull"]) == null);

			// read missing values with as casts
			test((obj["missing"] as Boolean) == false);
			test((obj["missing"] as int) == 0);
			test((obj["missing"] as uint) == 0);
			test((obj["missing"] as Number) == 0.0);
			test((obj["missing"] as String) == null);
			test((obj["missing"] as Object) == null);

			// read null values with as casts
			test((obj["dnull"] as Boolean) == false);
			test((obj["dnull"] as int) == 0);
			test((obj["dnull"] as uint) == 0);
			test((obj["dnull"] as Number) == 0.0);
			test((obj["dnull"] as String) == null);
			test((obj["dnull"] as Object) == null);


			// read with implicit casts
			var str:String = obj["dD"];
			test(str == null);
			str = obj["dF"];
			test(str == null);

			// read values out of object
			var e0:Boolean = obj["d0"];
			var e1:Boolean = obj["d1"];
			var e2:int  = obj["d2"];
			var e3:int  = obj["d3"];
			var e4:int  = obj["d4"];
			var e5:int  = obj["d5"];
			var e6:Number  = obj["d6"];
			var e7:Number  = obj["d7"];
			var e8:String  = obj["d8"];
			var e9:String  = obj["d9"];
			var eA:Object  = obj["dA"];
			var eB:Object  = obj["dB"];
			var eC:ObjectTest  = obj["dC"];
			var eD:*  = obj["dD"];
			var eE:*  = obj["dE"];
			var eF:*  = obj["dF"];
			var eG:Number = obj["dG"];
			var eH:String = obj["dH"];
			var eI:String = obj["dI"];

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
			test(dD == eD);
			test(dE == eE);
			test(dF == eF);
			test(isNaN(dG) && isNaN(eG));
			test(dH == eH);
			test(dI == eI);

			test(Boolean(d0) == Boolean(obj["d0"]));
			test(Boolean(d1) == Boolean(obj["d1"]));
			test(Boolean(d2) == Boolean(obj["d2"]));
			test(Boolean(d3) == Boolean(obj["d3"]));
			test(Boolean(d4) == Boolean(obj["d4"]));
			test(Boolean(d5) == Boolean(obj["d5"]));
			test(Boolean(d6) == Boolean(obj["d6"]));
			test(Boolean(d7) == Boolean(obj["d7"]));
			test(Boolean(d8) == Boolean(obj["d8"]));
			test(Boolean(d9) == Boolean(obj["d9"]));
			test(Boolean(dA) == Boolean(obj["dA"]));
			test(Boolean(dB) == Boolean(obj["dB"]));
			test(Boolean(dC) == Boolean(obj["dC"]));
			test(Boolean(dD) == Boolean(obj["dD"]));
			test(Boolean(dE) == Boolean(obj["dE"]));
			test(Boolean(dF) == Boolean(obj["dF"]));
			test(Boolean(dG) == Boolean(obj["dG"]));
			test(Boolean(dH) == Boolean(obj["dH"]));
			test(Boolean(dI) == Boolean(obj["dI"]));

			test(int(d0) == int(obj["d0"]));
			test(int(d1) == int(obj["d1"]));
			test(int(d2) == int(obj["d2"]));
			test(int(d3) == int(obj["d3"]));
			test(int(d4) == int(obj["d4"]));
			test(int(d5) == int(obj["d5"]));
			test(int(d6) == int(obj["d6"]));
			test(int(d7) == int(obj["d7"]));
			test(int(d8) == int(obj["d8"]));
			test(int(d9) == int(obj["d9"]));
			test(int(dA) == int(obj["dA"]));
			test(int(dB) == int(obj["dB"]));
			test(0 == int(obj["dC"]));
			test(int(dD) == int(obj["dD"]));
			test(int(dE) == int(obj["dE"]));
			test(int(dF) == int(obj["dF"]));
			test(int(dG) == int(obj["dG"]));
			test(int(dH) == int(obj["dH"]));
			test(int(dI) == int(obj["dI"]));

			test(uint(d0) == uint(obj["d0"]));
			test(uint(d1) == uint(obj["d1"]));
			test(uint(d2) == uint(obj["d2"]));
			test(uint(d3) == uint(obj["d3"]));
			test(uint(d4) == uint(obj["d4"]));
			test(uint(d5) == uint(obj["d5"]));
			test(uint(d6) == uint(obj["d6"]));
			test(uint(d7) == uint(obj["d7"]));
			test(uint(d8) == uint(obj["d8"]));
			test(uint(d9) == uint(obj["d9"]));
			test(uint(dA) == uint(obj["dA"]));
			test(uint(dB) == uint(obj["dB"]));
			test(0 == uint(obj["dC"]));
			test(uint(dD) == uint(obj["dD"]));
			test(uint(dE) == uint(obj["dE"]));
			test(uint(dF) == uint(obj["dF"]));
			test(uint(dG) == uint(obj["dG"]));
			test(uint(dH) == uint(obj["dH"]));
			test(uint(dI) == uint(obj["dI"]));


			test(Number(d0) == Number(obj["d0"]));
			test(Number(d1) == Number(obj["d1"]));
			test(Number(d2) == Number(obj["d2"]));
			test(Number(d3) == Number(obj["d3"]));
			test(Number(d4) == Number(obj["d4"]));
			test(Number(d5) == Number(obj["d5"]));
			test(Number(d6) == Number(obj["d6"]));
			test(Number(d7) == Number(obj["d7"]));
			test(isNaN(Number(d8)) && isNaN(Number(obj["d8"])));
			test(isNaN(Number(d9)) && isNaN(Number(obj["d9"])));
			test(isNaN(Number(dA)) && isNaN(Number(obj["dA"])));
			test(isNaN(Number(dB)) && isNaN(Number(obj["dB"])));
			test(isNaN(Number(obj["dC"])));
			test(Number(dD) == Number(obj["dD"]));
			test(Number(dE) == Number(obj["dE"]));
			test(isNaN(Number(dF)) && isNaN(Number(obj["dF"])));
			test(isNaN(Number(dG)) && isNaN(Number(obj["dG"])));
			test(Number(dH) == Number(obj["dH"]));
			test(Number(dI) == Number(obj["dI"]));


			test(String(d0) == String(obj["d0"]));
			test(String(d1) == String(obj["d1"]));
			test(String(d2) == String(obj["d2"]));
			test(String(d3) == String(obj["d3"]));
			test(String(d4) == String(obj["d4"]));
			test(String(d5) == String(obj["d5"]));
			test(String(d6) == String(obj["d6"]));
			test(String(d7) == String(obj["d7"]));
			test(String(d8) == String(obj["d8"]));
			test(String(d9) == String(obj["d9"]));
			test(String(dA) == String(obj["dA"]));
			test(String(dB) == String(obj["dB"]));
			test(String(dC) == String(obj["dC"]));
			test(String(dD) == String(obj["dD"]));
			test(String(dE) == String(obj["dE"]));
			test(String(dF) == String(obj["dF"]));
			test(String(dG) == String(obj["dG"]));
			test(String(dH) == String(obj["dH"]));
			test(String(dI) == String(obj["dI"]));


			// do testing of all "as" conversions
			test((d0 as Boolean) == (obj["d0"] as Boolean));
			test((d1 as Boolean) == (obj["d1"] as Boolean));
			test((d2 as Boolean) == (obj["d2"] as Boolean));
			test((d3 as Boolean) == (obj["d3"] as Boolean));
			test((d4 as Boolean) == (obj["d4"] as Boolean));
			test((d5 as Boolean) == (obj["d5"] as Boolean));
			test((d6 as Boolean) == (obj["d6"] as Boolean));
			test((d7 as Boolean) == (obj["d7"] as Boolean));
			test((d8 as Boolean) == (obj["d8"] as Boolean));
			test((d9 as Boolean) == (obj["d9"] as Boolean));
			test((dA as Boolean) == (obj["dA"] as Boolean));
			test((dB as Boolean) == (obj["dB"] as Boolean));
			test((dC as Boolean) == (obj["dC"] as Boolean));
			test((dD as Boolean) == (obj["dD"] as Boolean));
			test((dE as Boolean) == (obj["dE"] as Boolean));
			test((dF as Boolean) == (obj["dF"] as Boolean));
			test((dG as Boolean) == (obj["dG"] as Boolean));
			test((dH as Boolean) == (obj["dH"] as Boolean));
			test((dI as Boolean) == (obj["dI"] as Boolean));

			test((d0 as int) == (obj["d0"] as int));
			test((d1 as int) == (obj["d1"] as int));
			test((d2 as int) == (obj["d2"] as int));
			test((d3 as int) == (obj["d3"] as int));
			test((d4 as int) == (obj["d4"] as int));
			test((d5 as int) == (obj["d5"] as int));
			test((d6 as int) == (obj["d6"] as int));
			test((d7 as int) == (obj["d7"] as int));
			test((d8 as int) == (obj["d8"] as int));
			test((d9 as int) == (obj["d9"] as int));
			test((dA as int) == (obj["dA"] as int));
			test((dB as int) == (obj["dB"] as int));
			test((dC as int) == (obj["dC"] as int));
			test((dD as int) == (obj["dD"] as int));
			test((dE as int) == (obj["dE"] as int));
			test((dF as int) == (obj["dF"] as int));
			test((dG as int) == (obj["dG"] as int));
			test((dH as int) == (obj["dH"] as int));
			test((dI as int) == (obj["dI"] as int));

			test((d0 as uint) == (obj["d0"] as uint));
			test((d1 as uint) == (obj["d1"] as uint));
			test((d2 as uint) == (obj["d2"] as uint));
			test((d3 as uint) == (obj["d3"] as uint));
			test((d4 as uint) == (obj["d4"] as uint));
			test((d5 as uint) == (obj["d5"] as uint));
			test((d6 as uint) == (obj["d6"] as uint));
			test((d7 as uint) == (obj["d7"] as uint));
			test((d8 as uint) == (obj["d8"] as uint));
			test((d9 as uint) == (obj["d9"] as uint));
			test((dA as uint) == (obj["dA"] as uint));
			test((dB as uint) == (obj["dB"] as uint));
			test((dC as uint) == (obj["dC"] as uint));
			test((dD as uint) == (obj["dD"] as uint));
			test((dE as uint) == (obj["dE"] as uint));
			test((dF as uint) == (obj["dF"] as uint));
			test((dG as uint) == (obj["dG"] as uint));
			test((dH as uint) == (obj["dH"] as uint));
			test((dI as uint) == (obj["dI"] as uint));

			test((d0 as Number) == (obj["d0"] as Number));
			test((d1 as Number) == (obj["d1"] as Number));
			test((d2 as Number) == (obj["d2"] as Number));
			test((d3 as Number) == (obj["d3"] as Number));
			test((d4 as Number) == (obj["d4"] as Number));
			test((d5 as Number) == (obj["d5"] as Number));
			test((d6 as Number) == (obj["d6"] as Number));
			test((d7 as Number) == (obj["d7"] as Number));
			test((d8 as Number) == (obj["d8"] as Number));
			test((d9 as Number) == (obj["d9"] as Number));
			test((dA as Number) == (obj["dA"] as Number));
			test((dB as Number) == (obj["dB"] as Number));
			test((dC as Number) == (obj["dC"] as Number));
			test((dD as Number) == (obj["dD"] as Number));
			test((dE as Number) == (obj["dE"] as Number));
			test((dF as Number) == (obj["dF"] as Number));
			test(isNaN((dG as Number)) && isNaN((obj["dG"] as Number)));
			test((dH as Number) == (obj["dH"] as Number));
			test((dI as Number) == (obj["dI"] as Number));

			test((d0 as String) == (obj["d0"] as String));
			test((d1 as String) == (obj["d1"] as String));
			test((d2 as String) == (obj["d2"] as String));
			test((d3 as String) == (obj["d3"] as String));
			test((d4 as String) == (obj["d4"] as String));
			test((d5 as String) == (obj["d5"] as String));
			test((d6 as String) == (obj["d6"] as String));
			test((d7 as String) == (obj["d7"] as String));
			test((d8 as String) == (obj["d8"] as String));
			test((d9 as String) == (obj["d9"] as String));
			test((dA as String) == (obj["dA"] as String));
			test((dB as String) == (obj["dB"] as String));
			test((dC as String) == (obj["dC"] as String));
			test((dD as String) == (obj["dD"] as String));
			test((dE as String) == (obj["dE"] as String));
			test((dF as String) == (obj["dF"] as String));
			test((dG as String) == (obj["dG"] as String));
			test((dH as String) == (obj["dH"] as String));
			test((dI as String) == (obj["dI"] as String));		}

		public static function doTests():void
		{
			// expando object
			testObjectMember(new Object());
			testObjectIndex(new Object());

			// amf object
//			var amfObj:Object = new Amf.Amf3Object(Amf3ClassDef.Anonymous);
//			testObject(amfObj);
		}
	}
}
