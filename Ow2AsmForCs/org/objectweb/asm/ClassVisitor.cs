using System;
using java.lang;

namespace org.objectweb.asm {
public abstract class ClassVisitor {
protected readonly int api;
protected ClassVisitor cv;
public ClassVisitor(int api): this(api, null) {
}

public ClassVisitor(int api, ClassVisitor cv) {
if (api != Opcodes.ASM4 && api != Opcodes.ASM5) {
throw new IllegalArgumentException();
}
this.api = api;
this.cv = cv;
}

public virtual void visit(int version, int access, String name, String signature, String superName, String[] interfaces) {
if (cv != null) {
cv.visit(version, access, name, signature, superName, interfaces);
}
}
public virtual void visitSource(String source, String debug) {
if (cv != null) {
cv.visitSource(source, debug);
}
}
public virtual void visitOuterClass(String owner, String name, String desc) {
if (cv != null) {
cv.visitOuterClass(owner, name, desc);
}
}
public virtual AnnotationVisitor visitAnnotation(String desc, bool visible) {
if (cv != null) {
return cv.visitAnnotation(desc, visible);
}
return null;
}
public virtual AnnotationVisitor visitTypeAnnotation(int typeRef, TypePath typePath, String desc, bool visible) {
if (api < Opcodes.ASM5) {
throw new RuntimeException();
}
if (cv != null) {
return cv.visitTypeAnnotation(typeRef, typePath, desc, visible);
}
return null;
}
public virtual void visitAttribute(Attribute attr) {
if (cv != null) {
cv.visitAttribute(attr);
}
}
public virtual void visitInnerClass(String name, String outerName, String innerName, int access) {
if (cv != null) {
cv.visitInnerClass(name, outerName, innerName, access);
}
}
public virtual FieldVisitor visitField(int access, String name, String desc, String signature, Object value) {
if (cv != null) {
return cv.visitField(access, name, desc, signature, value);
}
return null;
}
public virtual MethodVisitor visitMethod(int access, String name, String desc, String signature, String[] exceptions) {
if (cv != null) {
return cv.visitMethod(access, name, desc, signature, exceptions);
}
return null;
}
public virtual void visitEnd() {
if (cv != null) {
cv.visitEnd();
}
}
}
}
