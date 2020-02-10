using System;
using java.lang;

namespace org.objectweb.asm {
public abstract class FieldVisitor {
protected readonly int api;
protected internal FieldVisitor fv;
public FieldVisitor(int api): this(api, null) {
}

public FieldVisitor(int api, FieldVisitor fv) {
if (api != Opcodes.ASM4 && api != Opcodes.ASM5) {
throw new IllegalArgumentException();
}
this.api = api;
this.fv = fv;
}

public virtual AnnotationVisitor visitAnnotation(String desc, bool visible) {
if (fv != null) {
return fv.visitAnnotation(desc, visible);
}
return null;
}
public virtual AnnotationVisitor visitTypeAnnotation(int typeRef, TypePath typePath, String desc, bool visible) {
if (api < Opcodes.ASM5) {
throw new RuntimeException();
}
if (fv != null) {
return fv.visitTypeAnnotation(typeRef, typePath, desc, visible);
}
return null;
}
public virtual void visitAttribute(Attribute attr) {
if (fv != null) {
fv.visitAttribute(attr);
}
}
public virtual void visitEnd() {
if (fv != null) {
fv.visitEnd();
}
}
}
}
