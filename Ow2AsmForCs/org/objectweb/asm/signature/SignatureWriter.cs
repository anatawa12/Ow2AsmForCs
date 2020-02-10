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

public override void visitFormalTypeParameter(String name) {
if (!hasFormals) {
hasFormals = true;
buf.append('<');
}
buf.append(name);
buf.append(':');
}
public override SignatureVisitor visitClassBound() {
return this;
}
public override SignatureVisitor visitInterfaceBound() {
buf.append(':');
return this;
}
public override SignatureVisitor visitSuperclass() {
endFormals();
return this;
}
public override SignatureVisitor visitInterface() {
return this;
}
public override SignatureVisitor visitParameterType() {
endFormals();
if (!hasParameters) {
hasParameters = true;
buf.append('(');
}
return this;
}
public override SignatureVisitor visitReturnType() {
endFormals();
if (!hasParameters) {
buf.append('(');
}
buf.append(')');
return this;
}
public override SignatureVisitor visitExceptionType() {
buf.append('^');
return this;
}
public override void visitBaseType(char descriptor) {
buf.append(descriptor);
}
public override void visitTypeVariable(String name) {
buf.append('T');
buf.append(name);
buf.append(';');
}
public override SignatureVisitor visitArrayType() {
buf.append('[');
return this;
}
public override void visitClassType(String name) {
buf.append('L');
buf.append(name);
argumentStack *= 2;
}
public override void visitInnerClassType(String name) {
endArguments();
buf.append('.');
buf.append(name);
argumentStack *= 2;
}
public override void visitTypeArgument() {
if (argumentStack % 2 == 0) {
++argumentStack;
buf.append('<');
}
buf.append('*');
}
public override SignatureVisitor visitTypeArgument(char wildcard) {
if (argumentStack % 2 == 0) {
++argumentStack;
buf.append('<');
}
if (wildcard != '=') {
buf.append(wildcard);
}
return this;
}
public override void visitEnd() {
endArguments();
buf.append(';');
}
public override String ToString() {
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
