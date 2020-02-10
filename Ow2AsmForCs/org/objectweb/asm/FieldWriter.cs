using System;
using java.lang;

namespace org.objectweb.asm {
sealed internal class FieldWriter: FieldVisitor {
private readonly ClassWriter cw;
private readonly int access;
private readonly int name;
private readonly int desc;
private int signature;
private int value;
private AnnotationWriter anns;
private AnnotationWriter ianns;
private AnnotationWriter tanns;
private AnnotationWriter itanns;
private Attribute attrs;
internal FieldWriter(ClassWriter cw, int access, String name, String desc, String signature, Object value): base(Opcodes.ASM5) {
if (cw.firstField == null) {
cw.firstField = this;
}
else {
cw.lastField.fv = this;
}
cw.lastField = this;
this.cw = cw;
this.access = access;
this.name = cw.newUTF8(name);
this.desc = cw.newUTF8(desc);
if (ClassReader.SIGNATURES && signature != null) {
this.signature = cw.newUTF8(signature);
}
if (value != null) {
this.value = cw.newConstItem(value).index;
}
}

public override AnnotationVisitor visitAnnotation(String desc, bool visible) {
if (!ClassReader.ANNOTATIONS) {
return null;
}
ByteVector bv = new ByteVector();
bv.putShort(cw.newUTF8(desc)).putShort(0);
AnnotationWriter aw = new AnnotationWriter(cw, true, bv, bv, 2);
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
public override AnnotationVisitor visitTypeAnnotation(int typeRef, TypePath typePath, String desc, bool visible) {
if (!ClassReader.ANNOTATIONS) {
return null;
}
ByteVector bv = new ByteVector();
AnnotationWriter.putTarget(typeRef, typePath, bv);
bv.putShort(cw.newUTF8(desc)).putShort(0);
AnnotationWriter aw = new AnnotationWriter(cw, true, bv, bv, bv.length - 2);
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
public override void visitAttribute(Attribute attr) {
attr.next = attrs;
attrs = attr;
}
public override void visitEnd() {
}
internal int getSize() {
int size = 8;
if (value != 0) {
cw.newUTF8("ConstantValue");
size += 8;
}
if ((access & Opcodes.ACC_SYNTHETIC) != 0) {
if ((cw.version & 0xFFFF) < Opcodes.V1_5 || (access & ClassWriter.ACC_SYNTHETIC_ATTRIBUTE) != 0) {
cw.newUTF8("Synthetic");
size += 6;
}
}
if ((access & Opcodes.ACC_DEPRECATED) != 0) {
cw.newUTF8("Deprecated");
size += 6;
}
if (ClassReader.SIGNATURES && signature != 0) {
cw.newUTF8("Signature");
size += 8;
}
if (ClassReader.ANNOTATIONS && anns != null) {
cw.newUTF8("RuntimeVisibleAnnotations");
size += 8 + anns.getSize();
}
if (ClassReader.ANNOTATIONS && ianns != null) {
cw.newUTF8("RuntimeInvisibleAnnotations");
size += 8 + ianns.getSize();
}
if (ClassReader.ANNOTATIONS && tanns != null) {
cw.newUTF8("RuntimeVisibleTypeAnnotations");
size += 8 + tanns.getSize();
}
if (ClassReader.ANNOTATIONS && itanns != null) {
cw.newUTF8("RuntimeInvisibleTypeAnnotations");
size += 8 + itanns.getSize();
}
if (attrs != null) {
size += attrs.getSize(cw, null, 0, -1, -1);
}
return size;
}
internal void put(ByteVector @out) {
int FACTOR = ClassWriter.TO_ACC_SYNTHETIC;
int mask = Opcodes.ACC_DEPRECATED | ClassWriter.ACC_SYNTHETIC_ATTRIBUTE | ((access & ClassWriter.ACC_SYNTHETIC_ATTRIBUTE) / FACTOR);
@out.putShort(access & ~mask).putShort(name).putShort(desc);
int attributeCount = 0;
if (value != 0) {
++attributeCount;
}
if ((access & Opcodes.ACC_SYNTHETIC) != 0) {
if ((cw.version & 0xFFFF) < Opcodes.V1_5 || (access & ClassWriter.ACC_SYNTHETIC_ATTRIBUTE) != 0) {
++attributeCount;
}
}
if ((access & Opcodes.ACC_DEPRECATED) != 0) {
++attributeCount;
}
if (ClassReader.SIGNATURES && signature != 0) {
++attributeCount;
}
if (ClassReader.ANNOTATIONS && anns != null) {
++attributeCount;
}
if (ClassReader.ANNOTATIONS && ianns != null) {
++attributeCount;
}
if (ClassReader.ANNOTATIONS && tanns != null) {
++attributeCount;
}
if (ClassReader.ANNOTATIONS && itanns != null) {
++attributeCount;
}
if (attrs != null) {
attributeCount += attrs.getCount();
}
@out.putShort(attributeCount);
if (value != 0) {
@out.putShort(cw.newUTF8("ConstantValue"));
@out.putInt(2).putShort(value);
}
if ((access & Opcodes.ACC_SYNTHETIC) != 0) {
if ((cw.version & 0xFFFF) < Opcodes.V1_5 || (access & ClassWriter.ACC_SYNTHETIC_ATTRIBUTE) != 0) {
@out.putShort(cw.newUTF8("Synthetic")).putInt(0);
}
}
if ((access & Opcodes.ACC_DEPRECATED) != 0) {
@out.putShort(cw.newUTF8("Deprecated")).putInt(0);
}
if (ClassReader.SIGNATURES && signature != 0) {
@out.putShort(cw.newUTF8("Signature"));
@out.putInt(2).putShort(signature);
}
if (ClassReader.ANNOTATIONS && anns != null) {
@out.putShort(cw.newUTF8("RuntimeVisibleAnnotations"));
anns.put(@out);
}
if (ClassReader.ANNOTATIONS && ianns != null) {
@out.putShort(cw.newUTF8("RuntimeInvisibleAnnotations"));
ianns.put(@out);
}
if (ClassReader.ANNOTATIONS && tanns != null) {
@out.putShort(cw.newUTF8("RuntimeVisibleTypeAnnotations"));
tanns.put(@out);
}
if (ClassReader.ANNOTATIONS && itanns != null) {
@out.putShort(cw.newUTF8("RuntimeInvisibleTypeAnnotations"));
itanns.put(@out);
}
if (attrs != null) {
attrs.put(cw, null, 0, -1, -1, @out);
}
}
}
}
