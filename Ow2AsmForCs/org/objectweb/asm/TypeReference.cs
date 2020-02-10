using System;
using java.lang;

namespace org.objectweb.asm {
public class TypeReference {
public readonly static int CLASS_TYPE_PARAMETER = 0x00;
public readonly static int METHOD_TYPE_PARAMETER = 0x01;
public readonly static int CLASS_EXTENDS = 0x10;
public readonly static int CLASS_TYPE_PARAMETER_BOUND = 0x11;
public readonly static int METHOD_TYPE_PARAMETER_BOUND = 0x12;
public readonly static int FIELD = 0x13;
public readonly static int METHOD_RETURN = 0x14;
public readonly static int METHOD_RECEIVER = 0x15;
public readonly static int METHOD_FORMAL_PARAMETER = 0x16;
public readonly static int THROWS = 0x17;
public readonly static int LOCAL_VARIABLE = 0x40;
public readonly static int RESOURCE_VARIABLE = 0x41;
public readonly static int EXCEPTION_PARAMETER = 0x42;
public readonly static int INSTANCEOF = 0x43;
public readonly static int NEW = 0x44;
public readonly static int CONSTRUCTOR_REFERENCE = 0x45;
public readonly static int METHOD_REFERENCE = 0x46;
public readonly static int CAST = 0x47;
public readonly static int CONSTRUCTOR_INVOCATION_TYPE_ARGUMENT = 0x48;
public readonly static int METHOD_INVOCATION_TYPE_ARGUMENT = 0x49;
public readonly static int CONSTRUCTOR_REFERENCE_TYPE_ARGUMENT = 0x4A;
public readonly static int METHOD_REFERENCE_TYPE_ARGUMENT = 0x4B;
private int value;
public TypeReference(int typeRef) {
this.value = typeRef;
}

public static TypeReference newTypeReference(int sort) {
return new TypeReference(sort << 24);
}
public static TypeReference newTypeParameterReference(int sort, int paramIndex) {
return new TypeReference((sort << 24) | (paramIndex << 16));
}
public static TypeReference newTypeParameterBoundReference(int sort, int paramIndex, int boundIndex) {
return new TypeReference((sort << 24) | (paramIndex << 16) | (boundIndex << 8));
}
public static TypeReference newSuperTypeReference(int itfIndex) {
itfIndex &= 0xFFFF;
return new TypeReference((CLASS_EXTENDS << 24) | (itfIndex << 8));
}
public static TypeReference newFormalParameterReference(int paramIndex) {
return new TypeReference((METHOD_FORMAL_PARAMETER << 24) | (paramIndex << 16));
}
public static TypeReference newExceptionReference(int exceptionIndex) {
return new TypeReference((THROWS << 24) | (exceptionIndex << 8));
}
public static TypeReference newTryCatchReference(int tryCatchBlockIndex) {
return new TypeReference((EXCEPTION_PARAMETER << 24) | (tryCatchBlockIndex << 8));
}
public static TypeReference newTypeArgumentReference(int sort, int argIndex) {
return new TypeReference((sort << 24) | argIndex);
}
public virtual int getSort() {
return value /*>>>*/ >> 24;
}
public virtual int getTypeParameterIndex() {
return (value & 0x00FF0000) >> 16;
}
public virtual int getTypeParameterBoundIndex() {
return (value & 0x0000FF00) >> 8;
}
public virtual int getSuperTypeIndex() {
return (short)((value & 0x00FFFF00) >> 8);
}
public virtual int getFormalParameterIndex() {
return (value & 0x00FF0000) >> 16;
}
public virtual int getExceptionIndex() {
return (value & 0x00FFFF00) >> 8;
}
public virtual int getTryCatchBlockIndex() {
return (value & 0x00FFFF00) >> 8;
}
public virtual int getTypeArgumentIndex() {
return value & 0xFF;
}
public virtual int getValue() {
return value;
}
}
}
