using System;
using java.lang;

namespace org.objectweb.asm {
public abstract class MethodVisitor {
protected readonly int api;
protected MethodVisitor mv;
public MethodVisitor(int api): this(api, null) {
}

public MethodVisitor(int api, MethodVisitor mv) {
if (api != Opcodes.ASM4 && api != Opcodes.ASM5) {
throw new IllegalArgumentException();
}
this.api = api;
this.mv = mv;
}

public virtual void visitParameter(String name, int access) {
if (api < Opcodes.ASM5) {
throw new RuntimeException();
}
if (mv != null) {
mv.visitParameter(name, access);
}
}
public virtual AnnotationVisitor visitAnnotationDefault() {
if (mv != null) {
return mv.visitAnnotationDefault();
}
return null;
}
public virtual AnnotationVisitor visitAnnotation(String desc, bool visible) {
if (mv != null) {
return mv.visitAnnotation(desc, visible);
}
return null;
}
public virtual AnnotationVisitor visitTypeAnnotation(int typeRef, TypePath typePath, String desc, bool visible) {
if (api < Opcodes.ASM5) {
throw new RuntimeException();
}
if (mv != null) {
return mv.visitTypeAnnotation(typeRef, typePath, desc, visible);
}
return null;
}
public virtual AnnotationVisitor visitParameterAnnotation(int parameter, String desc, bool visible) {
if (mv != null) {
return mv.visitParameterAnnotation(parameter, desc, visible);
}
return null;
}
public virtual void visitAttribute(Attribute attr) {
if (mv != null) {
mv.visitAttribute(attr);
}
}
public virtual void visitCode() {
if (mv != null) {
mv.visitCode();
}
}
public virtual void visitFrame(int type, int nLocal, Object[] local, int nStack, Object[] stack) {
if (mv != null) {
mv.visitFrame(type, nLocal, local, nStack, stack);
}
}
public virtual void visitInsn(int opcode) {
if (mv != null) {
mv.visitInsn(opcode);
}
}
public virtual void visitIntInsn(int opcode, int operand) {
if (mv != null) {
mv.visitIntInsn(opcode, operand);
}
}
public virtual void visitVarInsn(int opcode, int var) {
if (mv != null) {
mv.visitVarInsn(opcode, var);
}
}
public virtual void visitTypeInsn(int opcode, String type) {
if (mv != null) {
mv.visitTypeInsn(opcode, type);
}
}
public virtual void visitFieldInsn(int opcode, String owner, String name, String desc) {
if (mv != null) {
mv.visitFieldInsn(opcode, owner, name, desc);
}
}
public virtual void visitMethodInsn(int opcode, String owner, String name, String desc) {
if (api >= Opcodes.ASM5) {
bool itf = opcode == Opcodes.INVOKEINTERFACE;
visitMethodInsn(opcode, owner, name, desc, itf);
return;
}
if (mv != null) {
mv.visitMethodInsn(opcode, owner, name, desc);
}
}
public virtual void visitMethodInsn(int opcode, String owner, String name, String desc, bool itf) {
if (api < Opcodes.ASM5) {
if (itf != (opcode == Opcodes.INVOKEINTERFACE)) {
throw new IllegalArgumentException("INVOKESPECIAL/STATIC on interfaces require ASM 5");
}
visitMethodInsn(opcode, owner, name, desc);
return;
}
if (mv != null) {
mv.visitMethodInsn(opcode, owner, name, desc, itf);
}
}
public virtual void visitInvokeDynamicInsn(String name, String desc, Handle bsm, params Object[] bsmArgs) {
if (mv != null) {
mv.visitInvokeDynamicInsn(name, desc, bsm, bsmArgs);
}
}
public virtual void visitJumpInsn(int opcode, Label label) {
if (mv != null) {
mv.visitJumpInsn(opcode, label);
}
}
public virtual void visitLabel(Label label) {
if (mv != null) {
mv.visitLabel(label);
}
}
public virtual void visitLdcInsn(Object cst) {
if (mv != null) {
mv.visitLdcInsn(cst);
}
}
public virtual void visitIincInsn(int var, int increment) {
if (mv != null) {
mv.visitIincInsn(var, increment);
}
}
public virtual void visitTableSwitchInsn(int min, int max, Label dflt, params Label[] labels) {
if (mv != null) {
mv.visitTableSwitchInsn(min, max, dflt, labels);
}
}
public virtual void visitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels) {
if (mv != null) {
mv.visitLookupSwitchInsn(dflt, keys, labels);
}
}
public virtual void visitMultiANewArrayInsn(String desc, int dims) {
if (mv != null) {
mv.visitMultiANewArrayInsn(desc, dims);
}
}
public virtual AnnotationVisitor visitInsnAnnotation(int typeRef, TypePath typePath, String desc, bool visible) {
if (api < Opcodes.ASM5) {
throw new RuntimeException();
}
if (mv != null) {
return mv.visitInsnAnnotation(typeRef, typePath, desc, visible);
}
return null;
}
public virtual void visitTryCatchBlock(Label start, Label end, Label handler, String type) {
if (mv != null) {
mv.visitTryCatchBlock(start, end, handler, type);
}
}
public virtual AnnotationVisitor visitTryCatchAnnotation(int typeRef, TypePath typePath, String desc, bool visible) {
if (api < Opcodes.ASM5) {
throw new RuntimeException();
}
if (mv != null) {
return mv.visitTryCatchAnnotation(typeRef, typePath, desc, visible);
}
return null;
}
public virtual void visitLocalVariable(String name, String desc, String signature, Label start, Label end, int index) {
if (mv != null) {
mv.visitLocalVariable(name, desc, signature, start, end, index);
}
}
public virtual AnnotationVisitor visitLocalVariableAnnotation(int typeRef, TypePath typePath, Label[] start, Label[] end, int[] index, String desc, bool visible) {
if (api < Opcodes.ASM5) {
throw new RuntimeException();
}
if (mv != null) {
return mv.visitLocalVariableAnnotation(typeRef, typePath, start, end, index, desc, visible);
}
return null;
}
public virtual void visitLineNumber(int line, Label start) {
if (mv != null) {
mv.visitLineNumber(line, start);
}
}
public virtual void visitMaxs(int maxStack, int maxLocals) {
if (mv != null) {
mv.visitMaxs(maxStack, maxLocals);
}
}
public virtual void visitEnd() {
if (mv != null) {
mv.visitEnd();
}
}
}
}
