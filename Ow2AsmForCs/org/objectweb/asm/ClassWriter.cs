using System;
using java.lang;

namespace org.objectweb.asm {
public class ClassWriter: ClassVisitor {
public const int COMPUTE_MAXS = 1;
public const int COMPUTE_FRAMES = 2;
internal const int ACC_SYNTHETIC_ATTRIBUTE = 0x40000;
internal const int TO_ACC_SYNTHETIC = ACC_SYNTHETIC_ATTRIBUTE / Opcodes.ACC_SYNTHETIC;
internal const int NOARG_INSN = 0;
internal const int SBYTE_INSN = 1;
internal const int SHORT_INSN = 2;
internal const int VAR_INSN = 3;
internal const int IMPLVAR_INSN = 4;
internal const int TYPE_INSN = 5;
internal const int FIELDORMETH_INSN = 6;
internal const int ITFMETH_INSN = 7;
internal const int INDYMETH_INSN = 8;
internal const int LABEL_INSN = 9;
internal const int LABELW_INSN = 10;
internal const int LDC_INSN = 11;
internal const int LDCW_INSN = 12;
internal const int IINC_INSN = 13;
internal const int TABL_INSN = 14;
internal const int LOOK_INSN = 15;
internal const int MANA_INSN = 16;
internal const int WIDE_INSN = 17;
internal const int ASM_LABEL_INSN = 18;
internal const int F_INSERT = 256;
static readonly internal byte[] TYPE;
internal const int CLASS = 7;
internal const int FIELD = 9;
internal const int METH = 10;
internal const int IMETH = 11;
internal const int STR = 8;
internal const int INT = 3;
internal const int FLOAT = 4;
internal const int LONG = 5;
internal const int DOUBLE = 6;
internal const int NAME_TYPE = 12;
internal const int UTF8 = 1;
internal const int MTYPE = 16;
internal const int HANDLE = 15;
internal const int INDY = 18;
internal const int HANDLE_BASE = 20;
internal const int TYPE_NORMAL = 30;
internal const int TYPE_UNINIT = 31;
internal const int TYPE_MERGED = 32;
internal const int BSM = 33;
internal ClassReader cr;
internal int version;
internal int index;
readonly internal ByteVector pool;
internal Item[] items;
internal int threshold;
readonly internal Item key;
readonly internal Item key2;
readonly internal Item key3;
readonly internal Item key4;
internal Item[] typeTable;
private short typeCount;
private int access;
private int name;
internal String thisName;
private int signature;
private int superName;
private int interfaceCount;
private int[] interfaces;
private int sourceFile;
private ByteVector sourceDebug;
private int enclosingMethodOwner;
private int enclosingMethod;
private AnnotationWriter anns;
private AnnotationWriter ianns;
private AnnotationWriter tanns;
private AnnotationWriter itanns;
private Attribute attrs;
private int innerClassesCount;
private ByteVector innerClasses;
internal int bootstrapMethodsCount;
internal ByteVector bootstrapMethods;
internal FieldWriter firstField;
internal FieldWriter lastField;
internal MethodWriter firstMethod;
internal MethodWriter lastMethod;
private int compute;
internal bool hasAsmInsns;
static ClassWriter(){
int i;
byte[] b = new byte[220];
String s = "AAAAAAAAAAAAAAAABCLMMDDDDDEEEEEEEEEEEEEEEEEEEEAAAAAAAADD" + "DDDEEEEEEEEEEEEEEEEEEEEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" + "AAAAAAAAAAAAAAAAANAAAAAAAAAAAAAAAAAAAAJJJJJJJJJJJJJJJJDOPAA" + "AAAAGGGGGGGHIFBFAAFFAARQJJKKSSSSSSSSSSSSSSSSSS";
for (i = 0; i < b.length; ++i) {
b[i] = (byte)(s.charAt(i) - 'A');
}
TYPE = b;
}
public ClassWriter(int flags): base(Opcodes.ASM5) {
index = 1;
pool = new ByteVector();
items = new Item[256];
threshold = (int)(0.75d * items.length);
key = new Item();
key2 = new Item();
key3 = new Item();
key4 = new Item();
this.compute = (flags & COMPUTE_FRAMES) != 0 ? MethodWriter.FRAMES : ((flags & COMPUTE_MAXS) != 0 ? MethodWriter.MAXS : MethodWriter.NOTHING);
}

public ClassWriter(ClassReader classReader, int flags): this(flags) {
classReader.copyPool(this);
this.cr = classReader;
}

public void visit(int version, int access, String name, String signature, String superName, String[] interfaces) {
this.version = version;
this.access = access;
this.name = newClass(name);
thisName = name;
if (ClassReader.SIGNATURES && signature != null) {
this.signature = newUTF8(signature);
}
this.superName = superName == null ? 0 : newClass(superName);
if (interfaces != null && interfaces.length > 0) {
interfaceCount = interfaces.length;
this.interfaces = new int[interfaceCount];
for (int i = 0; i < interfaceCount; ++i) {
this.interfaces[i] = newClass(interfaces[i]);
}
}
}
public void visitSource(String file, String debug) {
if (file != null) {
sourceFile = newUTF8(file);
}
if (debug != null) {
sourceDebug = new ByteVector().encodeUTF8(debug, 0, Integer.MAX_VALUE);
}
}
public void visitOuterClass(String owner, String name, String desc) {
enclosingMethodOwner = newClass(owner);
if (name != null && desc != null) {
enclosingMethod = newNameType(name, desc);
}
}
public AnnotationVisitor visitAnnotation(String desc, bool visible) {
if (!ClassReader.ANNOTATIONS) {
return null;
}
ByteVector bv = new ByteVector();
bv.putShort(newUTF8(desc)).putShort(0);
AnnotationWriter aw = new AnnotationWriter(this, true, bv, bv, 2);
if (visible) {
aw.next = anns;
anns = aw;
}
else {
aw.next = ianns;
ianns = aw;
}
return aw;
}
public AnnotationVisitor visitTypeAnnotation(int typeRef, TypePath typePath, String desc, bool visible) {
if (!ClassReader.ANNOTATIONS) {
return null;
}
ByteVector bv = new ByteVector();
AnnotationWriter.putTarget(typeRef, typePath, bv);
bv.putShort(newUTF8(desc)).putShort(0);
AnnotationWriter aw = new AnnotationWriter(this, true, bv, bv, bv.length - 2);
if (visible) {
aw.next = tanns;
tanns = aw;
}
else {
aw.next = itanns;
itanns = aw;
}
return aw;
}
public void visitAttribute(Attribute attr) {
attr.next = attrs;
attrs = attr;
}
public void visitInnerClass(String name, String outerName, String innerName, int access) {
if (innerClasses == null) {
innerClasses = new ByteVector();
}
Item nameItem = newClassItem(name);
if (nameItem.intVal == 0) {
++innerClassesCount;
innerClasses.putShort(nameItem.index);
innerClasses.putShort(outerName == null ? 0 : newClass(outerName));
innerClasses.putShort(innerName == null ? 0 : newUTF8(innerName));
innerClasses.putShort(access);
nameItem.intVal = innerClassesCount;
}
else {
}
}
public FieldVisitor visitField(int access, String name, String desc, String signature, Object value) {
return new FieldWriter(this, access, name, desc, signature, value);
}
public MethodVisitor visitMethod(int access, String name, String desc, String signature, String[] exceptions) {
return new MethodWriter(this, access, name, desc, signature, exceptions, compute);
}
public void visitEnd() {
}
public virtual byte[] toByteArray() {
if (index > 0xFFFF) {
throw new RuntimeException("Class file too large!");
}
int size = 24 + 2 * interfaceCount;
int nbFields = 0;
FieldWriter fb = firstField;
while (fb != null){
++nbFields;
size += fb.getSize();
fb = (FieldWriter)fb.fv;
}
int nbMethods = 0;
MethodWriter mb = firstMethod;
while (mb != null){
++nbMethods;
size += mb.getSize();
mb = (MethodWriter)mb.mv;
}
int attributeCount = 0;
if (bootstrapMethods != null) {
++attributeCount;
size += 8 + bootstrapMethods.length;
newUTF8("BootstrapMethods");
}
if (ClassReader.SIGNATURES && signature != 0) {
++attributeCount;
size += 8;
newUTF8("Signature");
}
if (sourceFile != 0) {
++attributeCount;
size += 8;
newUTF8("SourceFile");
}
if (sourceDebug != null) {
++attributeCount;
size += sourceDebug.length + 6;
newUTF8("SourceDebugExtension");
}
if (enclosingMethodOwner != 0) {
++attributeCount;
size += 10;
newUTF8("EnclosingMethod");
}
if ((access & Opcodes.ACC_DEPRECATED) != 0) {
++attributeCount;
size += 6;
newUTF8("Deprecated");
}
if ((access & Opcodes.ACC_SYNTHETIC) != 0) {
if ((version & 0xFFFF) < Opcodes.V1_5 || (access & ACC_SYNTHETIC_ATTRIBUTE) != 0) {
++attributeCount;
size += 6;
newUTF8("Synthetic");
}
}
if (innerClasses != null) {
++attributeCount;
size += 8 + innerClasses.length;
newUTF8("InnerClasses");
}
if (ClassReader.ANNOTATIONS && anns != null) {
++attributeCount;
size += 8 + anns.getSize();
newUTF8("RuntimeVisibleAnnotations");
}
if (ClassReader.ANNOTATIONS && ianns != null) {
++attributeCount;
size += 8 + ianns.getSize();
newUTF8("RuntimeInvisibleAnnotations");
}
if (ClassReader.ANNOTATIONS && tanns != null) {
++attributeCount;
size += 8 + tanns.getSize();
newUTF8("RuntimeVisibleTypeAnnotations");
}
if (ClassReader.ANNOTATIONS && itanns != null) {
++attributeCount;
size += 8 + itanns.getSize();
newUTF8("RuntimeInvisibleTypeAnnotations");
}
if (attrs != null) {
attributeCount += attrs.getCount();
size += attrs.getSize(this, null, 0, -1, -1);
}
size += pool.length;
ByteVector @out = new ByteVector(size);
@out.putInt(0xCAFEBABE).putInt(version);
@out.putShort(index).putByteArray(pool.data, 0, pool.length);
int mask = Opcodes.ACC_DEPRECATED | ACC_SYNTHETIC_ATTRIBUTE | ((access & ACC_SYNTHETIC_ATTRIBUTE) / TO_ACC_SYNTHETIC);
@out.putShort(access & ~mask).putShort(name).putShort(superName);
@out.putShort(interfaceCount);
for (int i = 0; i < interfaceCount; ++i) {
@out.putShort(interfaces[i]);
}
@out.putShort(nbFields);
fb = firstField;
while (fb != null){
fb.put(@out);
fb = (FieldWriter)fb.fv;
}
@out.putShort(nbMethods);
mb = firstMethod;
while (mb != null){
mb.put(@out);
mb = (MethodWriter)mb.mv;
}
@out.putShort(attributeCount);
if (bootstrapMethods != null) {
@out.putShort(newUTF8("BootstrapMethods"));
@out.putInt(bootstrapMethods.length + 2).putShort(bootstrapMethodsCount);
@out.putByteArray(bootstrapMethods.data, 0, bootstrapMethods.length);
}
if (ClassReader.SIGNATURES && signature != 0) {
@out.putShort(newUTF8("Signature")).putInt(2).putShort(signature);
}
if (sourceFile != 0) {
@out.putShort(newUTF8("SourceFile")).putInt(2).putShort(sourceFile);
}
if (sourceDebug != null) {
int len = sourceDebug.length;
@out.putShort(newUTF8("SourceDebugExtension")).putInt(len);
@out.putByteArray(sourceDebug.data, 0, len);
}
if (enclosingMethodOwner != 0) {
@out.putShort(newUTF8("EnclosingMethod")).putInt(4);
@out.putShort(enclosingMethodOwner).putShort(enclosingMethod);
}
if ((access & Opcodes.ACC_DEPRECATED) != 0) {
@out.putShort(newUTF8("Deprecated")).putInt(0);
}
if ((access & Opcodes.ACC_SYNTHETIC) != 0) {
if ((version & 0xFFFF) < Opcodes.V1_5 || (access & ACC_SYNTHETIC_ATTRIBUTE) != 0) {
@out.putShort(newUTF8("Synthetic")).putInt(0);
}
}
if (innerClasses != null) {
@out.putShort(newUTF8("InnerClasses"));
@out.putInt(innerClasses.length + 2).putShort(innerClassesCount);
@out.putByteArray(innerClasses.data, 0, innerClasses.length);
}
if (ClassReader.ANNOTATIONS && anns != null) {
@out.putShort(newUTF8("RuntimeVisibleAnnotations"));
anns.put(@out);
}
if (ClassReader.ANNOTATIONS && ianns != null) {
@out.putShort(newUTF8("RuntimeInvisibleAnnotations"));
ianns.put(@out);
}
if (ClassReader.ANNOTATIONS && tanns != null) {
@out.putShort(newUTF8("RuntimeVisibleTypeAnnotations"));
tanns.put(@out);
}
if (ClassReader.ANNOTATIONS && itanns != null) {
@out.putShort(newUTF8("RuntimeInvisibleTypeAnnotations"));
itanns.put(@out);
}
if (attrs != null) {
attrs.put(this, null, 0, -1, -1, @out);
}
if (hasAsmInsns) {
anns = null;
ianns = null;
attrs = null;
innerClassesCount = 0;
innerClasses = null;
firstField = null;
lastField = null;
firstMethod = null;
lastMethod = null;
compute = MethodWriter.INSERTED_FRAMES;
hasAsmInsns = false;
new ClassReader(@out.data).accept(this, ClassReader.EXPAND_FRAMES | ClassReader.EXPAND_ASM_INSNS);
return toByteArray();
}
return @out.data;
}
internal virtual Item newConstItem(Object cst) {
if (cst is Integer) {
int val = ((Integer)cst).intValue();
return newInteger(val);
}
else if (cst is Byte) {
int val = ((Byte)cst).intValue();
return newInteger(val);
}
else if (cst is Character) {
int val = ((Character)cst).charValue();
return newInteger(val);
}
else if (cst is Short) {
int val = ((Short)cst).intValue();
return newInteger(val);
}
else if (cst is Boolean) {
int val = ((Boolean)cst).booleanValue() ? 1 : 0;
return newInteger(val);
}
else if (cst is Float) {
float val = ((Float)cst).floatValue();
return newFloat(val);
}
else if (cst is Long) {
long val = ((Long)cst).longValue();
return newLong(val);
}
else if (cst is Double) {
double val = ((Double)cst).doubleValue();
return newDouble(val);
}
else if (cst is String) {
return newString((String)cst);
}
else if (cst is Type) {
Type t = (Type)cst;
int s = t.getSort();
if (s == Type.OBJECT) {
return newClassItem(t.getInternalName());
}
else if (s == Type.METHOD) {
return newMethodTypeItem(t.getDescriptor());
}
else {
return newClassItem(t.getDescriptor());
}
}
else if (cst is Handle) {
Handle h = (Handle)cst;
return newHandleItem(h.tag, h.owner, h.name, h.desc, h.itf);
}
else {
throw new IllegalArgumentException("value " + cst);
}
}
public virtual int newConst(Object cst) {
return newConstItem(cst).index;
}
public virtual int newUTF8(String value) {
key.set(UTF8, value, null, null);
Item result = get(key);
if (result == null) {
pool.putByte(UTF8).putUTF8(value);
result = new Item(index++, key);
put(result);
}
return result.index;
}
internal virtual Item newClassItem(String value) {
key2.set(CLASS, value, null, null);
Item result = get(key2);
if (result == null) {
pool.put12(CLASS, newUTF8(value));
result = new Item(index++, key2);
put(result);
}
return result;
}
public virtual int newClass(String value) {
return newClassItem(value).index;
}
internal virtual Item newMethodTypeItem(String methodDesc) {
key2.set(MTYPE, methodDesc, null, null);
Item result = get(key2);
if (result == null) {
pool.put12(MTYPE, newUTF8(methodDesc));
result = new Item(index++, key2);
put(result);
}
return result;
}
public virtual int newMethodType(String methodDesc) {
return newMethodTypeItem(methodDesc).index;
}
internal virtual Item newHandleItem(int tag, String owner, String name, String desc, bool itf) {
key4.set(HANDLE_BASE + tag, owner, name, desc);
Item result = get(key4);
if (result == null) {
if (tag <= Opcodes.H_PUTSTATIC) {
put112(HANDLE, tag, newField(owner, name, desc));
}
else {
put112(HANDLE, tag, newMethod(owner, name, desc, itf));
}
result = new Item(index++, key4);
put(result);
}
return result;
}
public virtual int newHandle(int tag, String owner, String name, String desc) {
return newHandle(tag, owner, name, desc, tag == Opcodes.H_INVOKEINTERFACE);
}
public virtual int newHandle(int tag, String owner, String name, String desc, bool itf) {
return newHandleItem(tag, owner, name, desc, itf).index;
}
internal virtual Item newInvokeDynamicItem(String name, String desc, Handle bsm, params Object[] bsmArgs) {
ByteVector bootstrapMethods = this.bootstrapMethods;
if (bootstrapMethods == null) {
bootstrapMethods = this.bootstrapMethods = new ByteVector();
}
int position = bootstrapMethods.length;
int hashCode = bsm.hashCode();
bootstrapMethods.putShort(newHandle(bsm.tag, bsm.owner, bsm.name, bsm.desc, bsm.isInterface()));
int argsLength = bsmArgs.length;
bootstrapMethods.putShort(argsLength);
for (int i = 0; i < argsLength; i++) {
Object bsmArg = bsmArgs[i];
hashCode ^= bsmArg.hashCode();
bootstrapMethods.putShort(newConst(bsmArg));
}
byte[] data = bootstrapMethods.data;
int length = (1 + 1 + argsLength) << 1;
hashCode &= 0x7FFFFFFF;
Item result = items[hashCode % items.length];
loop:
while (result != null){
if (result.type != BSM || result.hashCode != hashCode) {
result = result.next;
continue;
}
int resultPosition = result.intVal;
for (int p = 0; p < length; p++) {
if (data[position + p] != data[resultPosition + p]) {
result = result.next;
continue /* label: loop */;
}
}
break;
}
int bootstrapMethodIndex;
if (result != null) {
bootstrapMethodIndex = result.index;
bootstrapMethods.length = position;
}
else {
bootstrapMethodIndex = bootstrapMethodsCount++;
result = new Item(bootstrapMethodIndex);
result.set(position, hashCode);
put(result);
}
key3.set(name, desc, bootstrapMethodIndex);
result = get(key3);
if (result == null) {
put122(INDY, bootstrapMethodIndex, newNameType(name, desc));
result = new Item(index++, key3);
put(result);
}
return result;
}
public virtual int newInvokeDynamic(String name, String desc, Handle bsm, params Object[] bsmArgs) {
return newInvokeDynamicItem(name, desc, bsm, bsmArgs).index;
}
internal virtual Item newFieldItem(String owner, String name, String desc) {
key3.set(FIELD, owner, name, desc);
Item result = get(key3);
if (result == null) {
put122(FIELD, newClass(owner), newNameType(name, desc));
result = new Item(index++, key3);
put(result);
}
return result;
}
public virtual int newField(String owner, String name, String desc) {
return newFieldItem(owner, name, desc).index;
}
internal virtual Item newMethodItem(String owner, String name, String desc, bool itf) {
int type = itf ? IMETH : METH;
key3.set(type, owner, name, desc);
Item result = get(key3);
if (result == null) {
put122(type, newClass(owner), newNameType(name, desc));
result = new Item(index++, key3);
put(result);
}
return result;
}
public virtual int newMethod(String owner, String name, String desc, bool itf) {
return newMethodItem(owner, name, desc, itf).index;
}
internal virtual Item newInteger(int value) {
key.set(value);
Item result = get(key);
if (result == null) {
pool.putByte(INT).putInt(value);
result = new Item(index++, key);
put(result);
}
return result;
}
internal virtual Item newFloat(float value) {
key.set(value);
Item result = get(key);
if (result == null) {
pool.putByte(FLOAT).putInt(key.intVal);
result = new Item(index++, key);
put(result);
}
return result;
}
internal virtual Item newLong(long value) {
key.set(value);
Item result = get(key);
if (result == null) {
pool.putByte(LONG).putLong(value);
result = new Item(index, key);
index += 2;
put(result);
}
return result;
}
internal virtual Item newDouble(double value) {
key.set(value);
Item result = get(key);
if (result == null) {
pool.putByte(DOUBLE).putLong(key.longVal);
result = new Item(index, key);
index += 2;
put(result);
}
return result;
}
private Item newString(String value) {
key2.set(STR, value, null, null);
Item result = get(key2);
if (result == null) {
pool.put12(STR, newUTF8(value));
result = new Item(index++, key2);
put(result);
}
return result;
}
public virtual int newNameType(String name, String desc) {
return newNameTypeItem(name, desc).index;
}
internal virtual Item newNameTypeItem(String name, String desc) {
key2.set(NAME_TYPE, name, desc, null);
Item result = get(key2);
if (result == null) {
put122(NAME_TYPE, newUTF8(name), newUTF8(desc));
result = new Item(index++, key2);
put(result);
}
return result;
}
internal virtual int addType(String type) {
key.set(TYPE_NORMAL, type, null, null);
Item result = get(key);
if (result == null) {
result = addType(key);
}
return result.index;
}
internal virtual int addUninitializedType(String type, int offset) {
key.type = TYPE_UNINIT;
key.intVal = offset;
key.strVal1 = type;
key.hashCode = 0x7FFFFFFF & (TYPE_UNINIT + type.hashCode() + offset);
Item result = get(key);
if (result == null) {
result = addType(key);
}
return result.index;
}
private Item addType(Item item) {
++typeCount;
Item result = new Item(typeCount, key);
put(result);
if (typeTable == null) {
typeTable = new Item[16];
}
if (typeCount == typeTable.length) {
Item[] newTable = new Item[2 * typeTable.length];
System.arraycopy(typeTable, 0, newTable, 0, typeTable.length);
typeTable = newTable;
}
typeTable[typeCount] = result;
return result;
}
internal virtual int getMergedType(int type1, int type2) {
key2.type = TYPE_MERGED;
key2.longVal = type1 | (((long)type2) << 32);
key2.hashCode = 0x7FFFFFFF & (TYPE_MERGED + type1 + type2);
Item result = get(key2);
if (result == null) {
String t = typeTable[type1].strVal1;
String u = typeTable[type2].strVal1;
key2.intVal = addType(getCommonSuperClass(t, u));
result = new Item((short)0, key2);
put(result);
}
return result.intVal;
}
protected virtual String getCommonSuperClass(String type1, String type2) {
Class</* ? (only) */ object> c, d;
ClassLoader classLoader = getClass().getClassLoader();
try {
c = Class.forName(type1.replace('/', '.'), false, classLoader);
d = Class.forName(type2.replace('/', '.'), false, classLoader);
}
catch (Exception e) {
throw new RuntimeException(e.toString());
}
if (c.isAssignableFrom(d)) {
return type1;
}
if (d.isAssignableFrom(c)) {
return type2;
}
if (c.isInterface() || d.isInterface()) {
return "java/lang/Object";
}
else {
do {
c = c.getSuperclass();
}
while (!c.isAssignableFrom(d));
return c.getName().replace('.', '/');
}
}
private Item get(Item key) {
Item i = items[key.hashCode % items.length];
while (i != null && (i.type != key.type || !key.isEqualTo(i))){
i = i.next;
}
return i;
}
private void put(Item i) {
if (index + typeCount > threshold) {
int ll = items.length;
int nl = ll * 2 + 1;
Item[] newItems = new Item[nl];
for (int l = ll - 1; l >= 0; --l) {
Item j = items[l];
while (j != null){
int index = j.hashCode % newItems.length;
Item k = j.next;
j.next = newItems[index];
newItems[index] = j;
j = k;
}
}
items = newItems;
threshold = (int)(nl * 0.75);
}
int index = i.hashCode % items.length;
i.next = items[index];
items[index] = i;
}
private void put122(int b, int s1, int s2) {
pool.put12(b, s1).putShort(s2);
}
private void put112(int b1, int b2, int s) {
pool.put11(b1, b2).putShort(s);
}
}
}
