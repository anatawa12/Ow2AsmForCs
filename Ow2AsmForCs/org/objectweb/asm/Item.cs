using System;
using java.lang;

namespace org.objectweb.asm {
sealed internal class Item {
internal int index;
internal int type;
internal int intVal;
internal long longVal;
internal String strVal1;
internal String strVal2;
internal String strVal3;
internal int hashCode;
internal Item next;
internal Item() {
}

internal Item(int index) {
this.index = index;
}

internal Item(int index, Item i) {
this.index = index;
type = i.type;
intVal = i.intVal;
longVal = i.longVal;
strVal1 = i.strVal1;
strVal2 = i.strVal2;
strVal3 = i.strVal3;
hashCode = i.hashCode;
}

internal void set(int intVal) {
this.type = ClassWriter.INT;
this.intVal = intVal;
this.hashCode = 0x7FFFFFFF & (type + intVal);
}
internal void set(long longVal) {
this.type = ClassWriter.LONG;
this.longVal = longVal;
this.hashCode = 0x7FFFFFFF & (type + (int)longVal);
}
internal void set(float floatVal) {
this.type = ClassWriter.FLOAT;
this.intVal = BitConverter.SingleToInt32Bits(floatVal);
this.hashCode = 0x7FFFFFFF & (type + (int)floatVal);
}
internal void set(double doubleVal) {
this.type = ClassWriter.DOUBLE;
this.longVal = BitConverter.DoubleToInt64Bits(doubleVal);
this.hashCode = 0x7FFFFFFF & (type + (int)doubleVal);
}
internal void set(int type, String strVal1, String strVal2, String strVal3) {
this.type = type;
this.strVal1 = strVal1;
this.strVal2 = strVal2;
this.strVal3 = strVal3;
switch (type) {
case ClassWriter.CLASS:
this.intVal = 0;
goto case ClassWriter.UTF8;
case ClassWriter.UTF8:
case ClassWriter.STR:
case ClassWriter.MTYPE:
case ClassWriter.TYPE_NORMAL:
hashCode = 0x7FFFFFFF & (type + strVal1.hashCode());
return;
case ClassWriter.NAME_TYPE:
{
hashCode = 0x7FFFFFFF & (type + strVal1.hashCode() * strVal2.hashCode());
return;
}
default:
hashCode = 0x7FFFFFFF & (type + strVal1.hashCode() * strVal2.hashCode() * strVal3.hashCode());
break;
}
}
internal void set(String name, String desc, int bsmIndex) {
this.type = ClassWriter.INDY;
this.longVal = bsmIndex;
this.strVal1 = name;
this.strVal2 = desc;
this.hashCode = 0x7FFFFFFF & (ClassWriter.INDY + bsmIndex * strVal1.hashCode() * strVal2.hashCode());
}
internal void set(int position, int hashCode) {
this.type = ClassWriter.BSM;
this.intVal = position;
this.hashCode = hashCode;
}
internal bool isEqualTo(Item i) {
switch (type) {
case ClassWriter.UTF8:
case ClassWriter.STR:
case ClassWriter.CLASS:
case ClassWriter.MTYPE:
case ClassWriter.TYPE_NORMAL:
return i.strVal1.equals(strVal1);
case ClassWriter.TYPE_MERGED:
case ClassWriter.LONG:
case ClassWriter.DOUBLE:
return i.longVal == longVal;
case ClassWriter.INT:
case ClassWriter.FLOAT:
return i.intVal == intVal;
case ClassWriter.TYPE_UNINIT:
return i.intVal == intVal && i.strVal1.equals(strVal1);
case ClassWriter.NAME_TYPE:
return i.strVal1.equals(strVal1) && i.strVal2.equals(strVal2);
case ClassWriter.INDY:
{
return i.longVal == longVal && i.strVal1.equals(strVal1) && i.strVal2.equals(strVal2);
}
default:
return i.strVal1.equals(strVal1) && i.strVal2.equals(strVal2) && i.strVal3.equals(strVal3);
}
}
}
}
