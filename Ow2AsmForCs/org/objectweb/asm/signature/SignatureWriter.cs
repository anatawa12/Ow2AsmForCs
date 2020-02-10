using System;
using java.lang;
using org.objectweb.asm;

namespace org.objectweb.asm.signature {
public class SignatureWriter: SignatureVisitor {
private readonly StringBuilder buf = new StringBuilder();
private bool hasFormals;
private bool hasParameters;
private int argumentStack;
public SignatureWriter(): base(Opcodes.ASM5) {
}

public virtual void visitFormalTypeParameter(String name) {
if (!hasFormals) {
hasFormals = true;
buf.append('<');
}
buf.append(name);
buf.append(':');
}
public virtual SignatureVisitor visitClassBound() {
return this;
}
public virtual SignatureVisitor visitInterfaceBound() {
buf.append(':');
return this;
}
public virtual SignatureVisitor visitSuperclass() {
endFormals();
return this;
}
public virtual SignatureVisitor visitInterface() {
return this;
}
public virtual SignatureVisitor visitParameterType() {
endFormals();
if (!hasParameters) {
hasParameters = true;
buf.append('(');
}
return this;
}
public virtual SignatureVisitor visitReturnType() {
endFormals();
if (!hasParameters) {
buf.append('(');
}
buf.append(')');
return this;
}
public virtual SignatureVisitor visitExceptionType() {
buf.append('^');
return this;
}
public virtual void visitBaseType(char descriptor) {
buf.append(descriptor);
}
public virtual void visitTypeVariable(String name) {
buf.append('T');
buf.append(name);
buf.append(';');
}
public virtual SignatureVisitor visitArrayType() {
buf.append('[');
return this;
}
public virtual void visitClassType(String name) {
buf.append('L');
buf.append(name);
argumentStack *= 2;
}
public virtual void visitInnerClassType(String name) {
endArguments();
buf.append('.');
buf.append(name);
argumentStack *= 2;
}
public virtual void visitTypeArgument() {
if (argumentStack % 2 == 0) {
++argumentStack;
buf.append('<');
}
buf.append('*');
}
public virtual SignatureVisitor visitTypeArgument(char wildcard) {
if (argumentStack % 2 == 0) {
++argumentStack;
buf.append('<');
}
if (wildcard != '=') {
buf.append(wildcard);
}
return this;
}
public virtual void visitEnd() {
endArguments();
buf.append(';');
}
public virtual String ToString() {
return buf.toString();
}
private void endFormals() {
if (hasFormals) {
hasFormals = false;
buf.append('>');
}
}
private void endArguments() {
if (argumentStack % 2 != 0) {
buf.append('>');
}
argumentStack /= 2;
}
}
}
