using System;
using java.lang;

namespace org.objectweb.asm {
public abstract class AnnotationVisitor {
protected readonly int api;
protected AnnotationVisitor av;
public AnnotationVisitor(int api): this(api, null) {
}

public AnnotationVisitor(int api, AnnotationVisitor av) {
if (api != Opcodes.ASM4 && api != Opcodes.ASM5) {
throw new IllegalArgumentException();
}
this.api = api;
this.av = av;
}

public virtual void visit(String name, Object value) {
if (av != null) {
av.visit(name, value);
}
}
public virtual void visitEnum(String name, String desc, String value) {
if (av != null) {
av.visitEnum(name, desc, value);
}
}
public virtual AnnotationVisitor visitAnnotation(String name, String desc) {
if (av != null) {
return av.visitAnnotation(name, desc);
}
return null;
}
public virtual AnnotationVisitor visitArray(String name) {
if (av != null) {
return av.visitArray(name);
}
return null;
}
public virtual void visitEnd() {
if (av != null) {
av.visitEnd();
}
}
}
}
