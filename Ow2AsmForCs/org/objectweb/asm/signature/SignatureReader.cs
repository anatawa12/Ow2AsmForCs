using System;
using java.lang;

namespace org.objectweb.asm.signature {
public class SignatureReader {
private readonly String signature;
public SignatureReader(String signature) {
this.signature = signature;
}

public virtual void accept(SignatureVisitor v) {
String signature = this.signature;
int len = signature.length();
int pos;
char c;
if (signature.charAt(0) == '<') {
pos = 2;
do {
int end = signature.indexOf(':', pos);
v.visitFormalTypeParameter(signature.substring(pos - 1, end));
pos = end + 1;
c = signature.charAt(pos);
if (c == 'L' || c == '[' || c == 'T') {
pos = parseType(signature, pos, v.visitClassBound());
}
while ((c = signature.charAt(pos++)) == ':'){
pos = parseType(signature, pos, v.visitInterfaceBound());
}
}
while (c != '>');
}
else {
pos = 0;
}
if (signature.charAt(pos) == '(') {
pos++;
while (signature.charAt(pos) != ')'){
pos = parseType(signature, pos, v.visitParameterType());
}
pos = parseType(signature, pos + 1, v.visitReturnType());
while (pos < len){
pos = parseType(signature, pos + 1, v.visitExceptionType());
}
}
else {
pos = parseType(signature, pos, v.visitSuperclass());
while (pos < len){
pos = parseType(signature, pos, v.visitInterface());
}
}
}
public virtual void acceptType(SignatureVisitor v) {
parseType(this.signature, 0, v);
}
private static int parseType(String signature, int pos, SignatureVisitor v) {
char c;
int start, end;
bool visited, inner;
String name;
switch (c = signature.charAt(pos++)) {
case 'Z':
case 'C':
case 'B':
case 'S':
case 'I':
case 'F':
case 'J':
case 'D':
case 'V':
v.visitBaseType(c);
return pos;
case '[':
return parseType(signature, pos, v.visitArrayType());
case 'T':
end = signature.indexOf(';', pos);
v.visitTypeVariable(signature.substring(pos, end));
return end + 1;
default:
start = pos;
visited = false;
inner = false;
for (; ; ) {
switch (c = signature.charAt(pos++)) {
case '.':
case ';':
if (!visited) {
name = signature.substring(start, pos - 1);
if (inner) {
v.visitInnerClassType(name);
}
else {
v.visitClassType(name);
}
}
if (c == ';') {
v.visitEnd();
return pos;
}
start = pos;
visited = false;
inner = true;
break;
case '<':
name = signature.substring(start, pos - 1);
if (inner) {
v.visitInnerClassType(name);
}
else {
v.visitClassType(name);
}
visited = true;
top:
for (; ; ) {
switch (c = signature.charAt(pos)) {
case '>':
goto top_break;
case '*':
++pos;
v.visitTypeArgument();
break;
case '+':
case '-':
pos = parseType(signature, pos + 1, v.visitTypeArgument(c));
break;
default:
pos = parseType(signature, pos, v.visitTypeArgument('='));
break;
}
}
top_break:;
}
}
}
}
}
}
