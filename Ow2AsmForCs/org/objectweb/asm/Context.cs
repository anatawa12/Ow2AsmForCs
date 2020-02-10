using System;
using java.lang;

namespace org.objectweb.asm {
internal class Context {
internal Attribute[] attrs;
internal int flags;
internal char[] buffer;
internal int[] bootstrapMethods;
internal int access;
internal String name;
internal String desc;
internal Label[] labels;
internal int typeRef;
internal TypePath typePath;
internal int offset;
internal Label[] start;
internal Label[] end;
internal int[] index;
internal int mode;
internal int localCount;
internal int localDiff;
internal Object[] local;
internal int stackCount;
internal Object[] stack;
}
}
