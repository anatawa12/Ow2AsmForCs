using System;
using java.lang;
using org.objectweb.asm;

namespace org.objectweb.asm.signature {
public abstract class SignatureVisitor {
public const char EXTENDS = '+';
public const char SUPER = '-';
public const char INSTANCEOF = '=';
protected readonly int api;
public SignatureVisitor(int api) {
if (api != Opcodes.ASM4 && api != Opcodes.ASM5) {
throw new IllegalArgumentException();
}
this.api = api;
}

public virtual void visitFormalTypeParameter(String name) {
}
public virtual SignatureVisitor visitClassBound() {
return this;
}
public virtual SignatureVisitor visitInterfaceBound() {
return this;
}
public virtual SignatureVisitor visitSuperclass() {
return this;
}
public virtual SignatureVisitor visitInterface() {
return this;
}
public virtual SignatureVisitor visitParameterType() {
return this;
}
public virtual SignatureVisitor visitReturnType() {
return this;
}
public virtual SignatureVisitor visitExceptionType() {
return this;
}
public virtual void visitBaseType(char descriptor) {
}
public virtual void visitTypeVariable(String name) {
}
public virtual SignatureVisitor visitArrayType() {
return this;
}
public virtual void visitClassType(String name) {
}
public virtual void visitInnerClassType(String name) {
}
public virtual void visitTypeArgument() {
}
public virtual SignatureVisitor visitTypeArgument(char wildcard) {
return this;
}
public virtual void visitEnd() {
}
}
}
