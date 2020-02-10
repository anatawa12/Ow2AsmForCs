using System;
using java.lang;
using java.io;

namespace org.objectweb.asm {
public class ClassReader {
internal const bool SIGNATURES = true;
internal const bool ANNOTATIONS = true;
internal const bool FRAMES = true;
internal const bool WRITER = true;
internal const bool RESIZE = true;
public const int SKIP_CODE = 1;
public const int SKIP_DEBUG = 2;
public const int SKIP_FRAMES = 4;
public const int EXPAND_FRAMES = 8;
internal const int EXPAND_ASM_INSNS = 256;
public readonly byte[] b;
private readonly int[] items;
private readonly String[] strings;
private readonly int maxStringLength;
public readonly int header;
public ClassReader(byte[] b): this(b, 0, b.Length) {
}

public ClassReader(byte[] b, int off, int len) {
this.b = b;
if (readShort(off + 6) > Opcodes.V1_8) {
throw new IllegalArgumentException();
}
items = new int[readUnsignedShort(off + 8)];
int n = items.Length;
strings = new String[n];
int max = 0;
int index = off + 10;
for (int i = 1; i < n; ++i) {
items[i] = index + 1;
int size;
switch (b[index]) {
case ClassWriter.FIELD:
case ClassWriter.METH:
case ClassWriter.IMETH:
case ClassWriter.INT:
case ClassWriter.FLOAT:
case ClassWriter.NAME_TYPE:
case ClassWriter.INDY:
size = 5;
break;
case ClassWriter.LONG:
case ClassWriter.DOUBLE:
size = 9;
++i;
break;
case ClassWriter.UTF8:
size = 3 + readUnsignedShort(index + 1);
if (size > max) {
max = size;
}
break;
case ClassWriter.HANDLE:
size = 4;
break;
default:
size = 3;
break;
}
index += size;
}
maxStringLength = max;
header = index;
}

public virtual int getAccess() {
return readUnsignedShort(header);
}
public virtual String getClassName() {
return readClass(header + 2, new char[maxStringLength]);
}
public virtual String getSuperName() {
return readClass(header + 4, new char[maxStringLength]);
}
public virtual String[] getInterfaces() {
int index = header + 6;
int n = readUnsignedShort(index);
String[] interfaces = new String[n];
if (n > 0) {
char[] buf = new char[maxStringLength];
for (int i = 0; i < n; ++i) {
index += 2;
interfaces[i] = readClass(index, buf);
}
}
return interfaces;
}
internal virtual void copyPool(ClassWriter classWriter) {
char[] buf = new char[maxStringLength];
int ll = items.Length;
Item[] items2 = new Item[ll];
for (int i = 1; i < ll; i++) {
int index = items[i];
int tag = b[index - 1];
Item item = new Item(i);
int nameType;
switch (tag) {
case ClassWriter.FIELD:
case ClassWriter.METH:
case ClassWriter.IMETH:
nameType = items[readUnsignedShort(index + 2)];
item.set(tag, readClass(index, buf), readUTF8(nameType, buf), readUTF8(nameType + 2, buf));
break;
case ClassWriter.INT:
item.set(readInt(index));
break;
case ClassWriter.FLOAT:
item.set(BitConverter.Int32BitsToSingle(readInt(index)));
break;
case ClassWriter.NAME_TYPE:
item.set(tag, readUTF8(index, buf), readUTF8(index + 2, buf), null);
break;
case ClassWriter.LONG:
item.set(readLong(index));
++i;
break;
case ClassWriter.DOUBLE:
item.set(BitConverter.Int64BitsToDouble(readLong(index)));
++i;
break;
case ClassWriter.UTF8:
{
String s = strings[i];
if (s == null) {
index = items[i];
s = strings[i] = readUTF(index + 2, readUnsignedShort(index), buf);
}
item.set(tag, s, null, null);
break;
}
case ClassWriter.HANDLE:
{
int fieldOrMethodRef = items[readUnsignedShort(index + 1)];
nameType = items[readUnsignedShort(fieldOrMethodRef + 2)];
item.set(ClassWriter.HANDLE_BASE + readByte(index), readClass(fieldOrMethodRef, buf), readUTF8(nameType, buf), readUTF8(nameType + 2, buf));
break;
}
case ClassWriter.INDY:
if (classWriter.bootstrapMethods == null) {
copyBootstrapMethods(classWriter, items2, buf);
}
nameType = items[readUnsignedShort(index + 2)];
item.set(readUTF8(nameType, buf), readUTF8(nameType + 2, buf), readUnsignedShort(index));
break;
default:
item.set(tag, readUTF8(index, buf), null, null);
break;
}
int index2 = item.hashCode % items2.Length;
item.next = items2[index2];
items2[index2] = item;
}
int off = items[1] - 1;
classWriter.pool.putByteArray(b, off, header - off);
classWriter.items = items2;
classWriter.threshold = (int)(0.75d * ll);
classWriter.index = ll;
}
private void copyBootstrapMethods(ClassWriter classWriter, Item[] items, char[] c) {
int u = getAttributes();
bool found = false;
for (int i = readUnsignedShort(u); i > 0; --i) {
String attrName = readUTF8(u + 2, c);
if ("BootstrapMethods".equals(attrName)) {
found = true;
break;
}
u += 6 + readInt(u + 4);
}
if (!found) {
return;
}
int boostrapMethodCount = readUnsignedShort(u + 8);
for (int j = 0, v = u + 10; j < boostrapMethodCount; j++) {
int position = v - u - 10;
int hashCode = readConst(readUnsignedShort(v), c).hashCode();
for (int k = readUnsignedShort(v + 2); k > 0; --k) {
hashCode ^= readConst(readUnsignedShort(v + 4), c).hashCode();
v += 2;
}
v += 4;
Item item = new Item(j);
item.set(position, hashCode & 0x7FFFFFFF);
int index = item.hashCode % items.Length;
item.next = items[index];
items[index] = item;
}
int attrSize = readInt(u + 4);
ByteVector bootstrapMethods = new ByteVector(attrSize + 62);
bootstrapMethods.putByteArray(b, u + 10, attrSize - 2);
classWriter.bootstrapMethodsCount = boostrapMethodCount;
classWriter.bootstrapMethods = bootstrapMethods;
}
public ClassReader(InputStream @is): this(readClass(@is, false)) {
}

private static byte[] readClass(InputStream @is, bool close) {
if (@is == null) {
throw new IOException("Class not found");
}
try {
byte[] b = new byte[@is.available()];
int len = 0;
while (true){
int n = @is.read(b, len, b.Length - len);
if (n == -1) {
if (len < b.Length) {
byte[] c = new byte[len];
SystemJ.arraycopy(b, 0, c, 0, len);
b = c;
}
return b;
}
len += n;
if (len == b.Length) {
int last = @is.read();
if (last < 0) {
return b;
}
byte[] c = new byte[b.Length + 1000];
SystemJ.arraycopy(b, 0, c, 0, len);
c[len++] = (byte)last;
b = c;
}
}
}
finally {
if (close) {
@is.close();
}
}
}
public virtual void accept(ClassVisitor classVisitor, int flags) {
accept(classVisitor, new Attribute[0], flags);
}
public virtual void accept(ClassVisitor classVisitor, Attribute[] attrs, int flags) {
int u = header;
char[] c = new char[maxStringLength];
Context context = new Context();
context.attrs = attrs;
context.flags = flags;
context.buffer = c;
int access = readUnsignedShort(u);
String name = readClass(u + 2, c);
String superClass = readClass(u + 4, c);
String[] interfaces = new String[readUnsignedShort(u + 6)];
u += 8;
for (int i = 0; i < interfaces.Length; ++i) {
interfaces[i] = readClass(u, c);
u += 2;
}
String signature = null;
String sourceFile = null;
String sourceDebug = null;
String enclosingOwner = null;
String enclosingName = null;
String enclosingDesc = null;
int anns = 0;
int ianns = 0;
int tanns = 0;
int itanns = 0;
int innerClasses = 0;
Attribute attributes = null;
u = getAttributes();
for (int i = readUnsignedShort(u); i > 0; --i) {
String attrName = readUTF8(u + 2, c);
if ("SourceFile".equals(attrName)) {
sourceFile = readUTF8(u + 8, c);
}
else if ("InnerClasses".equals(attrName)) {
innerClasses = u + 8;
}
else if ("EnclosingMethod".equals(attrName)) {
enclosingOwner = readClass(u + 8, c);
int item = readUnsignedShort(u + 10);
if (item != 0) {
enclosingName = readUTF8(items[item], c);
enclosingDesc = readUTF8(items[item] + 2, c);
}
}
else if (SIGNATURES && "Signature".equals(attrName)) {
signature = readUTF8(u + 8, c);
}
else if (ANNOTATIONS && "RuntimeVisibleAnnotations".equals(attrName)) {
anns = u + 8;
}
else if (ANNOTATIONS && "RuntimeVisibleTypeAnnotations".equals(attrName)) {
tanns = u + 8;
}
else if ("Deprecated".equals(attrName)) {
access |= Opcodes.ACC_DEPRECATED;
}
else if ("Synthetic".equals(attrName)) {
access |= Opcodes.ACC_SYNTHETIC | ClassWriter.ACC_SYNTHETIC_ATTRIBUTE;
}
else if ("SourceDebugExtension".equals(attrName)) {
int len = readInt(u + 4);
sourceDebug = readUTF(u + 8, len, new char[len]);
}
else if (ANNOTATIONS && "RuntimeInvisibleAnnotations".equals(attrName)) {
ianns = u + 8;
}
else if (ANNOTATIONS && "RuntimeInvisibleTypeAnnotations".equals(attrName)) {
itanns = u + 8;
}
else if ("BootstrapMethods".equals(attrName)) {
int[] bootstrapMethods = new int[readUnsignedShort(u + 8)];
for (int j = 0, v = u + 10; j < bootstrapMethods.Length; j++) {
bootstrapMethods[j] = v;
v += 2 + readUnsignedShort(v + 2) << 1;
}
context.bootstrapMethods = bootstrapMethods;
}
else {
Attribute attr = readAttribute(attrs, attrName, u + 8, readInt(u + 4), c, -1, null);
if (attr != null) {
attr.next = attributes;
attributes = attr;
}
}
u += 6 + readInt(u + 4);
}
classVisitor.visit(readInt(items[1] - 7), access, name, signature, superClass, interfaces);
if ((flags & SKIP_DEBUG) == 0 && (sourceFile != null || sourceDebug != null)) {
classVisitor.visitSource(sourceFile, sourceDebug);
}
if (enclosingOwner != null) {
classVisitor.visitOuterClass(enclosingOwner, enclosingName, enclosingDesc);
}
if (ANNOTATIONS && anns != 0) {
for (int i = readUnsignedShort(anns), v = anns + 2; i > 0; --i) {
v = readAnnotationValues(v + 2, c, true, classVisitor.visitAnnotation(readUTF8(v, c), true));
}
}
if (ANNOTATIONS && ianns != 0) {
for (int i = readUnsignedShort(ianns), v = ianns + 2; i > 0; --i) {
v = readAnnotationValues(v + 2, c, true, classVisitor.visitAnnotation(readUTF8(v, c), false));
}
}
if (ANNOTATIONS && tanns != 0) {
for (int i = readUnsignedShort(tanns), v = tanns + 2; i > 0; --i) {
v = readAnnotationTarget(context, v);
v = readAnnotationValues(v + 2, c, true, classVisitor.visitTypeAnnotation(context.typeRef, context.typePath, readUTF8(v, c), true));
}
}
if (ANNOTATIONS && itanns != 0) {
for (int i = readUnsignedShort(itanns), v = itanns + 2; i > 0; --i) {
v = readAnnotationTarget(context, v);
v = readAnnotationValues(v + 2, c, true, classVisitor.visitTypeAnnotation(context.typeRef, context.typePath, readUTF8(v, c), false));
}
}
while (attributes != null){
Attribute attr = attributes.next;
attributes.next = null;
classVisitor.visitAttribute(attributes);
attributes = attr;
}
if (innerClasses != 0) {
int v = innerClasses + 2;
for (int i = readUnsignedShort(innerClasses); i > 0; --i) {
classVisitor.visitInnerClass(readClass(v, c), readClass(v + 2, c), readUTF8(v + 4, c), readUnsignedShort(v + 6));
v += 8;
}
}
u = header + 10 + 2 * interfaces.Length;
for (int i = readUnsignedShort(u - 2); i > 0; --i) {
u = readField(classVisitor, context, u);
}
u += 2;
for (int i = readUnsignedShort(u - 2); i > 0; --i) {
u = readMethod(classVisitor, context, u);
}
classVisitor.visitEnd();
}
private int readField(ClassVisitor classVisitor, Context context, int u) {
char[] c = context.buffer;
int access = readUnsignedShort(u);
String name = readUTF8(u + 2, c);
String desc = readUTF8(u + 4, c);
u += 6;
String signature = null;
int anns = 0;
int ianns = 0;
int tanns = 0;
int itanns = 0;
Object value = null;
Attribute attributes = null;
for (int i = readUnsignedShort(u); i > 0; --i) {
String attrName = readUTF8(u + 2, c);
if ("ConstantValue".equals(attrName)) {
int item = readUnsignedShort(u + 8);
value = item == 0 ? null : readConst(item, c);
}
else if (SIGNATURES && "Signature".equals(attrName)) {
signature = readUTF8(u + 8, c);
}
else if ("Deprecated".equals(attrName)) {
access |= Opcodes.ACC_DEPRECATED;
}
else if ("Synthetic".equals(attrName)) {
access |= Opcodes.ACC_SYNTHETIC | ClassWriter.ACC_SYNTHETIC_ATTRIBUTE;
}
else if (ANNOTATIONS && "RuntimeVisibleAnnotations".equals(attrName)) {
anns = u + 8;
}
else if (ANNOTATIONS && "RuntimeVisibleTypeAnnotations".equals(attrName)) {
tanns = u + 8;
}
else if (ANNOTATIONS && "RuntimeInvisibleAnnotations".equals(attrName)) {
ianns = u + 8;
}
else if (ANNOTATIONS && "RuntimeInvisibleTypeAnnotations".equals(attrName)) {
itanns = u + 8;
}
else {
Attribute attr = readAttribute(context.attrs, attrName, u + 8, readInt(u + 4), c, -1, null);
if (attr != null) {
attr.next = attributes;
attributes = attr;
}
}
u += 6 + readInt(u + 4);
}
u += 2;
FieldVisitor fv = classVisitor.visitField(access, name, desc, signature, value);
if (fv == null) {
return u;
}
if (ANNOTATIONS && anns != 0) {
for (int i = readUnsignedShort(anns), v = anns + 2; i > 0; --i) {
v = readAnnotationValues(v + 2, c, true, fv.visitAnnotation(readUTF8(v, c), true));
}
}
if (ANNOTATIONS && ianns != 0) {
for (int i = readUnsignedShort(ianns), v = ianns + 2; i > 0; --i) {
v = readAnnotationValues(v + 2, c, true, fv.visitAnnotation(readUTF8(v, c), false));
}
}
if (ANNOTATIONS && tanns != 0) {
for (int i = readUnsignedShort(tanns), v = tanns + 2; i > 0; --i) {
v = readAnnotationTarget(context, v);
v = readAnnotationValues(v + 2, c, true, fv.visitTypeAnnotation(context.typeRef, context.typePath, readUTF8(v, c), true));
}
}
if (ANNOTATIONS && itanns != 0) {
for (int i = readUnsignedShort(itanns), v = itanns + 2; i > 0; --i) {
v = readAnnotationTarget(context, v);
v = readAnnotationValues(v + 2, c, true, fv.visitTypeAnnotation(context.typeRef, context.typePath, readUTF8(v, c), false));
}
}
while (attributes != null){
Attribute attr = attributes.next;
attributes.next = null;
fv.visitAttribute(attributes);
attributes = attr;
}
fv.visitEnd();
return u;
}
private int readMethod(ClassVisitor classVisitor, Context context, int u) {
char[] c = context.buffer;
context.access = readUnsignedShort(u);
context.name = readUTF8(u + 2, c);
context.desc = readUTF8(u + 4, c);
u += 6;
int code = 0;
int exception = 0;
String[] exceptions = null;
String signature = null;
int methodParameters = 0;
int anns = 0;
int ianns = 0;
int tanns = 0;
int itanns = 0;
int dann = 0;
int mpanns = 0;
int impanns = 0;
int firstAttribute = u;
Attribute attributes = null;
for (int i = readUnsignedShort(u); i > 0; --i) {
String attrName = readUTF8(u + 2, c);
if ("Code".equals(attrName)) {
if ((context.flags & SKIP_CODE) == 0) {
code = u + 8;
}
}
else if ("Exceptions".equals(attrName)) {
exceptions = new String[readUnsignedShort(u + 8)];
exception = u + 10;
for (int j = 0; j < exceptions.Length; ++j) {
exceptions[j] = readClass(exception, c);
exception += 2;
}
}
else if (SIGNATURES && "Signature".equals(attrName)) {
signature = readUTF8(u + 8, c);
}
else if ("Deprecated".equals(attrName)) {
context.access |= Opcodes.ACC_DEPRECATED;
}
else if (ANNOTATIONS && "RuntimeVisibleAnnotations".equals(attrName)) {
anns = u + 8;
}
else if (ANNOTATIONS && "RuntimeVisibleTypeAnnotations".equals(attrName)) {
tanns = u + 8;
}
else if (ANNOTATIONS && "AnnotationDefault".equals(attrName)) {
dann = u + 8;
}
else if ("Synthetic".equals(attrName)) {
context.access |= Opcodes.ACC_SYNTHETIC | ClassWriter.ACC_SYNTHETIC_ATTRIBUTE;
}
else if (ANNOTATIONS && "RuntimeInvisibleAnnotations".equals(attrName)) {
ianns = u + 8;
}
else if (ANNOTATIONS && "RuntimeInvisibleTypeAnnotations".equals(attrName)) {
itanns = u + 8;
}
else if (ANNOTATIONS && "RuntimeVisibleParameterAnnotations".equals(attrName)) {
mpanns = u + 8;
}
else if (ANNOTATIONS && "RuntimeInvisibleParameterAnnotations".equals(attrName)) {
impanns = u + 8;
}
else if ("MethodParameters".equals(attrName)) {
methodParameters = u + 8;
}
else {
Attribute attr = readAttribute(context.attrs, attrName, u + 8, readInt(u + 4), c, -1, null);
if (attr != null) {
attr.next = attributes;
attributes = attr;
}
}
u += 6 + readInt(u + 4);
}
u += 2;
MethodVisitor mv = classVisitor.visitMethod(context.access, context.name, context.desc, signature, exceptions);
if (mv == null) {
return u;
}
if (WRITER && mv is MethodWriter) {
MethodWriter mw = (MethodWriter)mv;
if (mw.cw.cr == this && signature == mw.signature) {
bool sameExceptions = false;
if (exceptions == null) {
sameExceptions = mw.exceptionCount == 0;
}
else if (exceptions.Length == mw.exceptionCount) {
sameExceptions = true;
for (int j = exceptions.Length - 1; j >= 0; --j) {
exception -= 2;
if (mw.exceptions[j] != readUnsignedShort(exception)) {
sameExceptions = false;
break;
}
}
}
if (sameExceptions) {
mw.classReaderOffset = firstAttribute;
mw.classReaderLength = u - firstAttribute;
return u;
}
}
}
if (methodParameters != 0) {
for (int i = b[methodParameters] & 0xFF, v = methodParameters + 1; i > 0; --i, v = v + 4) {
mv.visitParameter(readUTF8(v, c), readUnsignedShort(v + 2));
}
}
if (ANNOTATIONS && dann != 0) {
AnnotationVisitor dv = mv.visitAnnotationDefault();
readAnnotationValue(dann, c, null, dv);
if (dv != null) {
dv.visitEnd();
}
}
if (ANNOTATIONS && anns != 0) {
for (int i = readUnsignedShort(anns), v = anns + 2; i > 0; --i) {
v = readAnnotationValues(v + 2, c, true, mv.visitAnnotation(readUTF8(v, c), true));
}
}
if (ANNOTATIONS && ianns != 0) {
for (int i = readUnsignedShort(ianns), v = ianns + 2; i > 0; --i) {
v = readAnnotationValues(v + 2, c, true, mv.visitAnnotation(readUTF8(v, c), false));
}
}
if (ANNOTATIONS && tanns != 0) {
for (int i = readUnsignedShort(tanns), v = tanns + 2; i > 0; --i) {
v = readAnnotationTarget(context, v);
v = readAnnotationValues(v + 2, c, true, mv.visitTypeAnnotation(context.typeRef, context.typePath, readUTF8(v, c), true));
}
}
if (ANNOTATIONS && itanns != 0) {
for (int i = readUnsignedShort(itanns), v = itanns + 2; i > 0; --i) {
v = readAnnotationTarget(context, v);
v = readAnnotationValues(v + 2, c, true, mv.visitTypeAnnotation(context.typeRef, context.typePath, readUTF8(v, c), false));
}
}
if (ANNOTATIONS && mpanns != 0) {
readParameterAnnotations(mv, context, mpanns, true);
}
if (ANNOTATIONS && impanns != 0) {
readParameterAnnotations(mv, context, impanns, false);
}
while (attributes != null){
Attribute attr = attributes.next;
attributes.next = null;
mv.visitAttribute(attributes);
attributes = attr;
}
if (code != 0) {
mv.visitCode();
readCode(mv, context, code);
}
mv.visitEnd();
return u;
}
private void readCode(MethodVisitor mv, Context context, int u) {
byte[] b = this.b;
char[] c = context.buffer;
int maxStack = readUnsignedShort(u);
int maxLocals = readUnsignedShort(u + 2);
int codeLength = readInt(u + 4);
u += 8;
int codeStart = u;
int codeEnd = u + codeLength;
Label[] labels = context.labels = new Label[codeLength + 2];
readLabel(codeLength + 1, labels);
while (u < codeEnd){
int offset = u - codeStart;
int opcode = b[u] & 0xFF;
switch (ClassWriter.TYPE[opcode]) {
case ClassWriter.NOARG_INSN:
case ClassWriter.IMPLVAR_INSN:
u += 1;
break;
case ClassWriter.LABEL_INSN:
readLabel(offset + readShort(u + 1), labels);
u += 3;
break;
case ClassWriter.ASM_LABEL_INSN:
readLabel(offset + readUnsignedShort(u + 1), labels);
u += 3;
break;
case ClassWriter.LABELW_INSN:
readLabel(offset + readInt(u + 1), labels);
u += 5;
break;
case ClassWriter.WIDE_INSN:
opcode = b[u + 1] & 0xFF;
if (opcode == Opcodes.IINC) {
u += 6;
}
else {
u += 4;
}
break;
case ClassWriter.TABL_INSN:
u = u + 4 - (offset & 3);
readLabel(offset + readInt(u), labels);
for (int i = readInt(u + 8) - readInt(u + 4) + 1; i > 0; --i) {
readLabel(offset + readInt(u + 12), labels);
u += 4;
}
u += 12;
break;
case ClassWriter.LOOK_INSN:
u = u + 4 - (offset & 3);
readLabel(offset + readInt(u), labels);
for (int i = readInt(u + 4); i > 0; --i) {
readLabel(offset + readInt(u + 12), labels);
u += 8;
}
u += 8;
break;
case ClassWriter.VAR_INSN:
case ClassWriter.SBYTE_INSN:
case ClassWriter.LDC_INSN:
u += 2;
break;
case ClassWriter.SHORT_INSN:
case ClassWriter.LDCW_INSN:
case ClassWriter.FIELDORMETH_INSN:
case ClassWriter.TYPE_INSN:
case ClassWriter.IINC_INSN:
u += 3;
break;
case ClassWriter.ITFMETH_INSN:
case ClassWriter.INDYMETH_INSN:
u += 5;
break;
default:
u += 4;
break;
}
}
for (int i = readUnsignedShort(u); i > 0; --i) {
Label start = readLabel(readUnsignedShort(u + 2), labels);
Label end = readLabel(readUnsignedShort(u + 4), labels);
Label handler = readLabel(readUnsignedShort(u + 6), labels);
String type = readUTF8(items[readUnsignedShort(u + 8)], c);
mv.visitTryCatchBlock(start, end, handler, type);
u += 8;
}
u += 2;
int[] tanns = null;
int[] itanns = null;
int tann = 0;
int itann = 0;
int ntoff = -1;
int nitoff = -1;
int varTable = 0;
int varTypeTable = 0;
bool zip = true;
bool unzip = (context.flags & EXPAND_FRAMES) != 0;
int stackMap = 0;
int stackMapSize = 0;
int frameCount = 0;
Context frame = null;
Attribute attributes = null;
for (int i = readUnsignedShort(u); i > 0; --i) {
String attrName = readUTF8(u + 2, c);
if ("LocalVariableTable".equals(attrName)) {
if ((context.flags & SKIP_DEBUG) == 0) {
varTable = u + 8;
for (int j = readUnsignedShort(u + 8), v = u; j > 0; --j) {
int label = readUnsignedShort(v + 10);
if (labels[label] == null) {
readLabel(label, labels).status |= Label.DEBUG;
}
label += readUnsignedShort(v + 12);
if (labels[label] == null) {
readLabel(label, labels).status |= Label.DEBUG;
}
v += 10;
}
}
}
else if ("LocalVariableTypeTable".equals(attrName)) {
varTypeTable = u + 8;
}
else if ("LineNumberTable".equals(attrName)) {
if ((context.flags & SKIP_DEBUG) == 0) {
for (int j = readUnsignedShort(u + 8), v = u; j > 0; --j) {
int label = readUnsignedShort(v + 10);
if (labels[label] == null) {
readLabel(label, labels).status |= Label.DEBUG;
}
Label l = labels[label];
while (l.line > 0){
if (l.next == null) {
l.next = new Label();
}
l = l.next;
}
l.line = readUnsignedShort(v + 12);
v += 4;
}
}
}
else if (ANNOTATIONS && "RuntimeVisibleTypeAnnotations".equals(attrName)) {
tanns = readTypeAnnotations(mv, context, u + 8, true);
ntoff = tanns.Length == 0 || readByte(tanns[0]) < 0x43 ? -1 : readUnsignedShort(tanns[0] + 1);
}
else if (ANNOTATIONS && "RuntimeInvisibleTypeAnnotations".equals(attrName)) {
itanns = readTypeAnnotations(mv, context, u + 8, false);
nitoff = itanns.Length == 0 || readByte(itanns[0]) < 0x43 ? -1 : readUnsignedShort(itanns[0] + 1);
}
else if (FRAMES && "StackMapTable".equals(attrName)) {
if ((context.flags & SKIP_FRAMES) == 0) {
stackMap = u + 10;
stackMapSize = readInt(u + 4);
frameCount = readUnsignedShort(u + 8);
}
}
else if (FRAMES && "StackMap".equals(attrName)) {
if ((context.flags & SKIP_FRAMES) == 0) {
zip = false;
stackMap = u + 10;
stackMapSize = readInt(u + 4);
frameCount = readUnsignedShort(u + 8);
}
}
else {
for (int j = 0; j < context.attrs.Length; ++j) {
if (context.attrs[j].type.equals(attrName)) {
Attribute attr = context.attrs[j].read(this, u + 8, readInt(u + 4), c, codeStart - 8, labels);
if (attr != null) {
attr.next = attributes;
attributes = attr;
}
}
}
}
u += 6 + readInt(u + 4);
}
u += 2;
if (FRAMES && stackMap != 0) {
frame = context;
frame.offset = -1;
frame.mode = 0;
frame.localCount = 0;
frame.localDiff = 0;
frame.stackCount = 0;
frame.local = new Object[maxLocals];
frame.stack = new Object[maxStack];
if (unzip) {
getImplicitFrame(context);
}
for (int i = stackMap; i < stackMap + stackMapSize - 2; ++i) {
if (b[i] == 8) {
int v = readUnsignedShort(i + 1);
if (v >= 0 && v < codeLength) {
if ((b[codeStart + v] & 0xFF) == Opcodes.NEW) {
readLabel(v, labels);
}
}
}
}
}
if ((context.flags & EXPAND_ASM_INSNS) != 0) {
mv.visitFrame(Opcodes.F_NEW, maxLocals, null, 0, null);
}
int opcodeDelta = (context.flags & EXPAND_ASM_INSNS) == 0 ? -33 : 0;
u = codeStart;
while (u < codeEnd){
int offset = u - codeStart;
Label l = labels[offset];
if (l != null) {
Label next = l.next;
l.next = null;
mv.visitLabel(l);
if ((context.flags & SKIP_DEBUG) == 0 && l.line > 0) {
mv.visitLineNumber(l.line, l);
while (next != null){
mv.visitLineNumber(next.line, l);
next = next.next;
}
}
}
while (FRAMES && frame != null && (frame.offset == offset || frame.offset == -1)){
if (frame.offset != -1) {
if (!zip || unzip) {
mv.visitFrame(Opcodes.F_NEW, frame.localCount, frame.local, frame.stackCount, frame.stack);
}
else {
mv.visitFrame(frame.mode, frame.localDiff, frame.local, frame.stackCount, frame.stack);
}
}
if (frameCount > 0) {
stackMap = readFrame(stackMap, zip, unzip, frame);
--frameCount;
}
else {
frame = null;
}
}
int opcode = b[u] & 0xFF;
switch (ClassWriter.TYPE[opcode]) {
case ClassWriter.NOARG_INSN:
mv.visitInsn(opcode);
u += 1;
break;
case ClassWriter.IMPLVAR_INSN:
if (opcode > Opcodes.ISTORE) {
opcode -= 59;
mv.visitVarInsn(Opcodes.ISTORE + (opcode >> 2), opcode & 0x3);
}
else {
opcode -= 26;
mv.visitVarInsn(Opcodes.ILOAD + (opcode >> 2), opcode & 0x3);
}
u += 1;
break;
case ClassWriter.LABEL_INSN:
mv.visitJumpInsn(opcode, labels[offset + readShort(u + 1)]);
u += 3;
break;
case ClassWriter.LABELW_INSN:
mv.visitJumpInsn(opcode + opcodeDelta, labels[offset + readInt(u + 1)]);
u += 5;
break;
case ClassWriter.ASM_LABEL_INSN:
{
opcode = opcode < 218 ? opcode - 49 : opcode - 20;
Label target = labels[offset + readUnsignedShort(u + 1)];
if (opcode == Opcodes.GOTO || opcode == Opcodes.JSR) {
mv.visitJumpInsn(opcode + 33, target);
}
else {
opcode = opcode <= 166 ? ((opcode + 1) ^ 1) - 1 : opcode ^ 1;
Label endif = new Label();
mv.visitJumpInsn(opcode, endif);
mv.visitJumpInsn(200, target);
mv.visitLabel(endif);
if (FRAMES && stackMap != 0 && (frame == null || frame.offset != offset + 3)) {
mv.visitFrame(ClassWriter.F_INSERT, 0, null, 0, null);
}
}
u += 3;
break;
}
case ClassWriter.WIDE_INSN:
opcode = b[u + 1] & 0xFF;
if (opcode == Opcodes.IINC) {
mv.visitIincInsn(readUnsignedShort(u + 2), readShort(u + 4));
u += 6;
}
else {
mv.visitVarInsn(opcode, readUnsignedShort(u + 2));
u += 4;
}
break;
case ClassWriter.TABL_INSN:
{
u = u + 4 - (offset & 3);
int label = offset + readInt(u);
int min = readInt(u + 4);
int max = readInt(u + 8);
Label[] table = new Label[max - min + 1];
u += 12;
for (int i = 0; i < table.Length; ++i) {
table[i] = labels[offset + readInt(u)];
u += 4;
}
mv.visitTableSwitchInsn(min, max, labels[label], table);
break;
}
case ClassWriter.LOOK_INSN:
{
u = u + 4 - (offset & 3);
int label = offset + readInt(u);
int len = readInt(u + 4);
int[] keys = new int[len];
Label[] values = new Label[len];
u += 8;
for (int i = 0; i < len; ++i) {
keys[i] = readInt(u);
values[i] = labels[offset + readInt(u + 4)];
u += 8;
}
mv.visitLookupSwitchInsn(labels[label], keys, values);
break;
}
case ClassWriter.VAR_INSN:
mv.visitVarInsn(opcode, b[u + 1] & 0xFF);
u += 2;
break;
case ClassWriter.SBYTE_INSN:
mv.visitIntInsn(opcode, b[u + 1]);
u += 2;
break;
case ClassWriter.SHORT_INSN:
mv.visitIntInsn(opcode, readShort(u + 1));
u += 3;
break;
case ClassWriter.LDC_INSN:
mv.visitLdcInsn(readConst(b[u + 1] & 0xFF, c));
u += 2;
break;
case ClassWriter.LDCW_INSN:
mv.visitLdcInsn(readConst(readUnsignedShort(u + 1), c));
u += 3;
break;
case ClassWriter.FIELDORMETH_INSN:
case ClassWriter.ITFMETH_INSN:
{
int cpIndex = items[readUnsignedShort(u + 1)];
bool itf = b[cpIndex - 1] == ClassWriter.IMETH;
String iowner = readClass(cpIndex, c);
cpIndex = items[readUnsignedShort(cpIndex + 2)];
String iname = readUTF8(cpIndex, c);
String idesc = readUTF8(cpIndex + 2, c);
if (opcode < Opcodes.INVOKEVIRTUAL) {
mv.visitFieldInsn(opcode, iowner, iname, idesc);
}
else {
mv.visitMethodInsn(opcode, iowner, iname, idesc, itf);
}
if (opcode == Opcodes.INVOKEINTERFACE) {
u += 5;
}
else {
u += 3;
}
break;
}
case ClassWriter.INDYMETH_INSN:
{
int cpIndex = items[readUnsignedShort(u + 1)];
int bsmIndex = context.bootstrapMethods[readUnsignedShort(cpIndex)];
Handle bsm = (Handle)readConst(readUnsignedShort(bsmIndex), c);
int bsmArgCount = readUnsignedShort(bsmIndex + 2);
Object[] bsmArgs = new Object[bsmArgCount];
bsmIndex += 4;
for (int i = 0; i < bsmArgCount; i++) {
bsmArgs[i] = readConst(readUnsignedShort(bsmIndex), c);
bsmIndex += 2;
}
cpIndex = items[readUnsignedShort(cpIndex + 2)];
String iname = readUTF8(cpIndex, c);
String idesc = readUTF8(cpIndex + 2, c);
mv.visitInvokeDynamicInsn(iname, idesc, bsm, bsmArgs);
u += 5;
break;
}
case ClassWriter.TYPE_INSN:
mv.visitTypeInsn(opcode, readClass(u + 1, c));
u += 3;
break;
case ClassWriter.IINC_INSN:
mv.visitIincInsn(b[u + 1] & 0xFF, b[u + 2]);
u += 3;
break;
default:
mv.visitMultiANewArrayInsn(readClass(u + 1, c), b[u + 3] & 0xFF);
u += 4;
break;
}
while (tanns != null && tann < tanns.Length && ntoff <= offset){
if (ntoff == offset) {
int v = readAnnotationTarget(context, tanns[tann]);
readAnnotationValues(v + 2, c, true, mv.visitInsnAnnotation(context.typeRef, context.typePath, readUTF8(v, c), true));
}
ntoff = ++tann >= tanns.Length || readByte(tanns[tann]) < 0x43 ? -1 : readUnsignedShort(tanns[tann] + 1);
}
while (itanns != null && itann < itanns.Length && nitoff <= offset){
if (nitoff == offset) {
int v = readAnnotationTarget(context, itanns[itann]);
readAnnotationValues(v + 2, c, true, mv.visitInsnAnnotation(context.typeRef, context.typePath, readUTF8(v, c), false));
}
nitoff = ++itann >= itanns.Length || readByte(itanns[itann]) < 0x43 ? -1 : readUnsignedShort(itanns[itann] + 1);
}
}
if (labels[codeLength] != null) {
mv.visitLabel(labels[codeLength]);
}
if ((context.flags & SKIP_DEBUG) == 0 && varTable != 0) {
int[] typeTable = null;
if (varTypeTable != 0) {
u = varTypeTable + 2;
typeTable = new int[readUnsignedShort(varTypeTable) * 3];
for (int i = typeTable.Length; i > 0; ) {
typeTable[--i] = u + 6;
typeTable[--i] = readUnsignedShort(u + 8);
typeTable[--i] = readUnsignedShort(u);
u += 10;
}
}
u = varTable + 2;
for (int i = readUnsignedShort(varTable); i > 0; --i) {
int start = readUnsignedShort(u);
int length = readUnsignedShort(u + 2);
int index = readUnsignedShort(u + 8);
String vsignature = null;
if (typeTable != null) {
for (int j = 0; j < typeTable.Length; j += 3) {
if (typeTable[j] == start && typeTable[j + 1] == index) {
vsignature = readUTF8(typeTable[j + 2], c);
break;
}
}
}
mv.visitLocalVariable(readUTF8(u + 4, c), readUTF8(u + 6, c), vsignature, labels[start], labels[start + length], index);
u += 10;
}
}
if (tanns != null) {
for (int i = 0; i < tanns.Length; ++i) {
if ((readByte(tanns[i]) >> 1) == (0x40 >> 1)) {
int v = readAnnotationTarget(context, tanns[i]);
v = readAnnotationValues(v + 2, c, true, mv.visitLocalVariableAnnotation(context.typeRef, context.typePath, context.start, context.end, context.index, readUTF8(v, c), true));
}
}
}
if (itanns != null) {
for (int i = 0; i < itanns.Length; ++i) {
if ((readByte(itanns[i]) >> 1) == (0x40 >> 1)) {
int v = readAnnotationTarget(context, itanns[i]);
v = readAnnotationValues(v + 2, c, true, mv.visitLocalVariableAnnotation(context.typeRef, context.typePath, context.start, context.end, context.index, readUTF8(v, c), false));
}
}
}
while (attributes != null){
Attribute attr = attributes.next;
attributes.next = null;
mv.visitAttribute(attributes);
attributes = attr;
}
mv.visitMaxs(maxStack, maxLocals);
}
private int[] readTypeAnnotations(MethodVisitor mv, Context context, int u, bool visible) {
char[] c = context.buffer;
int[] offsets = new int[readUnsignedShort(u)];
u += 2;
for (int i = 0; i < offsets.Length; ++i) {
offsets[i] = u;
int target = readInt(u);
switch (target /*>>>*/ >> 24) {
case 0x00:
case 0x01:
case 0x16:
u += 2;
break;
case 0x13:
case 0x14:
case 0x15:
u += 1;
break;
case 0x40:
case 0x41:
for (int j = readUnsignedShort(u + 1); j > 0; --j) {
int start = readUnsignedShort(u + 3);
int length = readUnsignedShort(u + 5);
readLabel(start, context.labels);
readLabel(start + length, context.labels);
u += 6;
}
u += 3;
break;
case 0x47:
case 0x48:
case 0x49:
case 0x4A:
case 0x4B:
u += 4;
break;
default:
u += 3;
break;
}
int pathLength = readByte(u);
if ((target /*>>>*/ >> 24) == 0x42) {
TypePath path = pathLength == 0 ? null : new TypePath(b, u);
u += 1 + 2 * pathLength;
u = readAnnotationValues(u + 2, c, true, mv.visitTryCatchAnnotation(target, path, readUTF8(u, c), visible));
}
else {
u = readAnnotationValues(u + 3 + 2 * pathLength, c, true, null);
}
}
return offsets;
}
private int readAnnotationTarget(Context context, int u) {
int target = readInt(u);
switch (target /*>>>*/ >> 24) {
case 0x00:
case 0x01:
case 0x16:
target &= unchecked((int)0xFFFF0000);
u += 2;
break;
case 0x13:
case 0x14:
case 0x15:
target &= unchecked((int)0xFF000000);
u += 1;
break;
case 0x40:
case 0x41:
{
target &= unchecked((int)0xFF000000);
int n = readUnsignedShort(u + 1);
context.start = new Label[n];
context.end = new Label[n];
context.index = new int[n];
u += 3;
for (int i = 0; i < n; ++i) {
int start = readUnsignedShort(u);
int length = readUnsignedShort(u + 2);
context.start[i] = readLabel(start, context.labels);
context.end[i] = readLabel(start + length, context.labels);
context.index[i] = readUnsignedShort(u + 4);
u += 6;
}
break;
}
case 0x47:
case 0x48:
case 0x49:
case 0x4A:
case 0x4B:
target &= unchecked((int)0xFF0000FF);
u += 4;
break;
default:
target &= unchecked((target /*>>>*/ >> 24) < 0x43 ? (int)0xFFFFFF00 : (int)0xFF000000);
u += 3;
break;
}
int pathLength = readByte(u);
context.typeRef = target;
context.typePath = pathLength == 0 ? null : new TypePath(b, u);
return u + 1 + 2 * pathLength;
}
private void readParameterAnnotations(MethodVisitor mv, Context context, int v, bool visible) {
int i;
int n = b[v++] & 0xFF;
int synthetics = Type.getArgumentTypes(context.desc).Length - n;
AnnotationVisitor av;
for (i = 0; i < synthetics; ++i) {
av = mv.visitParameterAnnotation(i, "Ljava/lang/Synthetic;", false);
if (av != null) {
av.visitEnd();
}
}
char[] c = context.buffer;
for (; i < n + synthetics; ++i) {
int j = readUnsignedShort(v);
v += 2;
for (; j > 0; --j) {
av = mv.visitParameterAnnotation(i, readUTF8(v, c), visible);
v = readAnnotationValues(v + 2, c, true, av);
}
}
}
private int readAnnotationValues(int v, char[] buf, bool named, AnnotationVisitor av) {
int i = readUnsignedShort(v);
v += 2;
if (named) {
for (; i > 0; --i) {
v = readAnnotationValue(v + 2, buf, readUTF8(v, buf), av);
}
}
else {
for (; i > 0; --i) {
v = readAnnotationValue(v, buf, null, av);
}
}
if (av != null) {
av.visitEnd();
}
return v;
}
private int readAnnotationValue(int v, char[] buf, String name, AnnotationVisitor av) {
int i;
if (av == null) {
switch (b[v] & 0xFF) {
case 'e':
return v + 5;
case '@':
return readAnnotationValues(v + 3, buf, true, null);
case '[':
return readAnnotationValues(v + 1, buf, false, null);
default:
return v + 3;
}
}
switch (b[v++] & 0xFF) {
case 'I':
case 'J':
case 'F':
case 'D':
av.visit(name, readConst(readUnsignedShort(v), buf));
v += 2;
break;
case 'B':
av.visit(name, (byte)readInt(items[readUnsignedShort(v)]));
v += 2;
break;
case 'Z':
av.visit(name, readInt(items[readUnsignedShort(v)]) != 0);
v += 2;
break;
case 'S':
av.visit(name, (short)readInt(items[readUnsignedShort(v)]));
v += 2;
break;
case 'C':
av.visit(name, (char)readInt(items[readUnsignedShort(v)]));
v += 2;
break;
case 's':
av.visit(name, readUTF8(v, buf));
v += 2;
break;
case 'e':
av.visitEnum(name, readUTF8(v, buf), readUTF8(v + 2, buf));
v += 4;
break;
case 'c':
av.visit(name, Type.getType(readUTF8(v, buf)));
v += 2;
break;
case '@':
v = readAnnotationValues(v + 2, buf, true, av.visitAnnotation(name, readUTF8(v, buf)));
break;
case '[':
int size = readUnsignedShort(v);
v += 2;
if (size == 0) {
return readAnnotationValues(v - 2, buf, false, av.visitArray(name));
}
switch (this.b[v++] & 0xFF) {
case 'B':
byte[] bv = new byte[size];
for (i = 0; i < size; i++) {
bv[i] = (byte)readInt(items[readUnsignedShort(v)]);
v += 3;
}
av.visit(name, bv);
--v;
break;
case 'Z':
bool[] zv = new bool[size];
for (i = 0; i < size; i++) {
zv[i] = readInt(items[readUnsignedShort(v)]) != 0;
v += 3;
}
av.visit(name, zv);
--v;
break;
case 'S':
short[] sv = new short[size];
for (i = 0; i < size; i++) {
sv[i] = (short)readInt(items[readUnsignedShort(v)]);
v += 3;
}
av.visit(name, sv);
--v;
break;
case 'C':
char[] cv = new char[size];
for (i = 0; i < size; i++) {
cv[i] = (char)readInt(items[readUnsignedShort(v)]);
v += 3;
}
av.visit(name, cv);
--v;
break;
case 'I':
int[] iv = new int[size];
for (i = 0; i < size; i++) {
iv[i] = readInt(items[readUnsignedShort(v)]);
v += 3;
}
av.visit(name, iv);
--v;
break;
case 'J':
long[] lv = new long[size];
for (i = 0; i < size; i++) {
lv[i] = readLong(items[readUnsignedShort(v)]);
v += 3;
}
av.visit(name, lv);
--v;
break;
case 'F':
float[] fv = new float[size];
for (i = 0; i < size; i++) {
fv[i] = BitConverter.Int32BitsToSingle(readInt(items[readUnsignedShort(v)]));
v += 3;
}
av.visit(name, fv);
--v;
break;
case 'D':
double[] dv = new double[size];
for (i = 0; i < size; i++) {
dv[i] = BitConverter.Int64BitsToDouble(readLong(items[readUnsignedShort(v)]));
v += 3;
}
av.visit(name, dv);
--v;
break;
default:
v = readAnnotationValues(v - 3, buf, false, av.visitArray(name));
}
}
return v;
}
private void getImplicitFrame(Context frame) {
String desc = frame.desc;
Object[] locals = frame.local;
int local = 0;
if ((frame.access & Opcodes.ACC_STATIC) == 0) {
if ("<init>".equals(frame.name)) {
locals[local++] = Opcodes.UNINITIALIZED_THIS;
}
else {
locals[local++] = readClass(header + 2, frame.buffer);
}
}
int i = 1;
loop:
while (true){
int j = i;
switch (desc.charAt(i++)) {
case 'Z':
case 'C':
case 'B':
case 'S':
case 'I':
locals[local++] = Opcodes.INTEGER;
break;
case 'F':
locals[local++] = Opcodes.FLOAT;
break;
case 'J':
locals[local++] = Opcodes.LONG;
break;
case 'D':
locals[local++] = Opcodes.DOUBLE;
break;
case '[':
while (desc.charAt(i) == '['){
++i;
}
if (desc.charAt(i) == 'L') {
++i;
while (desc.charAt(i) != ';'){
++i;
}
}
locals[local++] = desc.substring(j, ++i);
break;
case 'L':
while (desc.charAt(i) != ';'){
++i;
}
locals[local++] = desc.substring(j + 1, i++);
break;
default:
break /* label: loop */;
}
}
frame.localCount = local;
}
private int readFrame(int stackMap, bool zip, bool unzip, Context frame) {
char[] c = frame.buffer;
Label[] labels = frame.labels;
int tag;
int delta;
if (zip) {
tag = b[stackMap++] & 0xFF;
}
else {
tag = MethodWriter.FULL_FRAME;
frame.offset = -1;
}
frame.localDiff = 0;
if (tag < MethodWriter.SAME_LOCALS_1_STACK_ITEM_FRAME) {
delta = tag;
frame.mode = Opcodes.F_SAME;
frame.stackCount = 0;
}
else if (tag < MethodWriter.RESERVED) {
delta = tag - MethodWriter.SAME_LOCALS_1_STACK_ITEM_FRAME;
stackMap = readFrameType(frame.stack, 0, stackMap, c, labels);
frame.mode = Opcodes.F_SAME1;
frame.stackCount = 1;
}
else {
delta = readUnsignedShort(stackMap);
stackMap += 2;
if (tag == MethodWriter.SAME_LOCALS_1_STACK_ITEM_FRAME_EXTENDED) {
stackMap = readFrameType(frame.stack, 0, stackMap, c, labels);
frame.mode = Opcodes.F_SAME1;
frame.stackCount = 1;
}
else if (tag >= MethodWriter.CHOP_FRAME && tag < MethodWriter.SAME_FRAME_EXTENDED) {
frame.mode = Opcodes.F_CHOP;
frame.localDiff = MethodWriter.SAME_FRAME_EXTENDED - tag;
frame.localCount -= frame.localDiff;
frame.stackCount = 0;
}
else if (tag == MethodWriter.SAME_FRAME_EXTENDED) {
frame.mode = Opcodes.F_SAME;
frame.stackCount = 0;
}
else if (tag < MethodWriter.FULL_FRAME) {
int local = unzip ? frame.localCount : 0;
for (int i = tag - MethodWriter.SAME_FRAME_EXTENDED; i > 0; i--) {
stackMap = readFrameType(frame.local, local++, stackMap, c, labels);
}
frame.mode = Opcodes.F_APPEND;
frame.localDiff = tag - MethodWriter.SAME_FRAME_EXTENDED;
frame.localCount += frame.localDiff;
frame.stackCount = 0;
}
else {
frame.mode = Opcodes.F_FULL;
int n = readUnsignedShort(stackMap);
stackMap += 2;
frame.localDiff = n;
frame.localCount = n;
for (int local = 0; n > 0; n--) {
stackMap = readFrameType(frame.local, local++, stackMap, c, labels);
}
n = readUnsignedShort(stackMap);
stackMap += 2;
frame.stackCount = n;
for (int stack = 0; n > 0; n--) {
stackMap = readFrameType(frame.stack, stack++, stackMap, c, labels);
}
}
}
frame.offset += delta + 1;
readLabel(frame.offset, labels);
return stackMap;
}
private int readFrameType(Object[] frame, int index, int v, char[] buf, Label[] labels) {
int type = b[v++] & 0xFF;
switch (type) {
case 0:
frame[index] = Opcodes.TOP;
break;
case 1:
frame[index] = Opcodes.INTEGER;
break;
case 2:
frame[index] = Opcodes.FLOAT;
break;
case 3:
frame[index] = Opcodes.DOUBLE;
break;
case 4:
frame[index] = Opcodes.LONG;
break;
case 5:
frame[index] = Opcodes.NULL;
break;
case 6:
frame[index] = Opcodes.UNINITIALIZED_THIS;
break;
case 7:
frame[index] = readClass(v, buf);
v += 2;
break;
default:
frame[index] = readLabel(readUnsignedShort(v), labels);
v += 2;
}
return v;
}
protected virtual Label readLabel(int offset, Label[] labels) {
if (labels[offset] == null) {
labels[offset] = new Label();
}
return labels[offset];
}
private int getAttributes() {
int u = header + 8 + readUnsignedShort(header + 6) * 2;
for (int i = readUnsignedShort(u); i > 0; --i) {
for (int j = readUnsignedShort(u + 8); j > 0; --j) {
u += 6 + readInt(u + 12);
}
u += 8;
}
u += 2;
for (int i = readUnsignedShort(u); i > 0; --i) {
for (int j = readUnsignedShort(u + 8); j > 0; --j) {
u += 6 + readInt(u + 12);
}
u += 8;
}
return u + 2;
}
private Attribute readAttribute(Attribute[] attrs, String type, int off, int len, char[] buf, int codeOff, Label[] labels) {
for (int i = 0; i < attrs.Length; ++i) {
if (attrs[i].type.equals(type)) {
return attrs[i].read(this, off, len, buf, codeOff, labels);
}
}
return new Attribute(type).read(this, off, len, null, -1, null);
}
public virtual int getItemCount() {
return items.Length;
}
public virtual int getItem(int item) {
return items[item];
}
public virtual int getMaxStringLength() {
return maxStringLength;
}
public virtual int readByte(int index) {
return b[index] & 0xFF;
}
public virtual int readUnsignedShort(int index) {
byte[] b = this.b;
return ((b[index] & 0xFF) << 8) | (b[index + 1] & 0xFF);
}
public virtual short readShort(int index) {
byte[] b = this.b;
return (short)(((b[index] & 0xFF) << 8) | (b[index + 1] & 0xFF));
}
public virtual int readInt(int index) {
byte[] b = this.b;
return ((b[index] & 0xFF) << 24) | ((b[index + 1] & 0xFF) << 16) | ((b[index + 2] & 0xFF) << 8) | (b[index + 3] & 0xFF);
}
public virtual long readLong(int index) {
long l1 = readInt(index);
long l0 = readInt(index + 4) & 0xFFFFFFFFL;
return (l1 << 32) | l0;
}
public virtual String readUTF8(int index, char[] buf) {
int item = readUnsignedShort(index);
if (index == 0 || item == 0) {
return null;
}
String s = strings[item];
if (s != null) {
return s;
}
index = items[item];
return strings[item] = readUTF(index + 2, readUnsignedShort(index), buf);
}
private String readUTF(int index, int utfLen, char[] buf) {
int endIndex = index + utfLen;
byte[] b = this.b;
int strLen = 0;
int c;
int st = 0;
char cc = (char)0;
while (index < endIndex){
c = b[index++];
switch (st) {
case 0:
c = c & 0xFF;
if (c < 0x80) {
buf[strLen++] = (char)c;
}
else if (c < 0xE0 && c > 0xBF) {
cc = (char)(c & 0x1F);
st = 1;
}
else {
cc = (char)(c & 0x0F);
st = 2;
}
break;
case 1:
buf[strLen++] = (char)((cc << 6) | (c & 0x3F));
st = 0;
break;
case 2:
cc = (char)((cc << 6) | (c & 0x3F));
st = 1;
break;
}
}
return new String(buf, 0, strLen);
}
public virtual String readClass(int index, char[] buf) {
return readUTF8(items[readUnsignedShort(index)], buf);
}
public virtual Object readConst(int item, char[] buf) {
int index = items[item];
switch (b[index - 1]) {
case ClassWriter.INT:
return readInt(index);
case ClassWriter.FLOAT:
return BitConverter.Int32BitsToSingle(readInt(index));
case ClassWriter.LONG:
return readLong(index);
case ClassWriter.DOUBLE:
return BitConverter.Int64BitsToDouble(readLong(index));
case ClassWriter.CLASS:
return Type.getObjectType(readUTF8(index, buf));
case ClassWriter.STR:
return readUTF8(index, buf);
case ClassWriter.MTYPE:
return Type.getMethodType(readUTF8(index, buf));
default:
int tag = readByte(index);
int[] items = this.items;
int cpIndex = items[readUnsignedShort(index + 1)];
bool itf = b[cpIndex - 1] == ClassWriter.IMETH;
String owner = readClass(cpIndex, buf);
cpIndex = items[readUnsignedShort(cpIndex + 2)];
String name = readUTF8(cpIndex, buf);
String desc = readUTF8(cpIndex + 2, buf);
return new Handle(tag, owner, name, desc, itf);
}
}
}
}
