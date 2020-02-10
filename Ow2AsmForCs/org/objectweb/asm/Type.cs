using System;
using java.lang;
using java.lang.reflect;

namespace org.objectweb.asm {
public class Type {
public static readonly int VOID = 0;
public static readonly int BOOLEAN = 1;
public static readonly int CHAR = 2;
public static readonly int BYTE = 3;
public static readonly int SHORT = 4;
public static readonly int INT = 5;
public static readonly int FLOAT = 6;
public static readonly int LONG = 7;
public static readonly int DOUBLE = 8;
public static readonly int ARRAY = 9;
public static readonly int OBJECT = 10;
public static readonly int METHOD = 11;
public static readonly Type VOID_TYPE = new Type(VOID, null, ('V' << 24) | (5 << 16) | (0 << 8) | 0, 1);
public static readonly Type BOOLEAN_TYPE = new Type(BOOLEAN, null, ('Z' << 24) | (0 << 16) | (5 << 8) | 1, 1);
public static readonly Type CHAR_TYPE = new Type(CHAR, null, ('C' << 24) | (0 << 16) | (6 << 8) | 1, 1);
public static readonly Type BYTE_TYPE = new Type(BYTE, null, ('B' << 24) | (0 << 16) | (5 << 8) | 1, 1);
public static readonly Type SHORT_TYPE = new Type(SHORT, null, ('S' << 24) | (0 << 16) | (7 << 8) | 1, 1);
public static readonly Type INT_TYPE = new Type(INT, null, ('I' << 24) | (0 << 16) | (0 << 8) | 1, 1);
public static readonly Type FLOAT_TYPE = new Type(FLOAT, null, ('F' << 24) | (2 << 16) | (2 << 8) | 1, 1);
public static readonly Type LONG_TYPE = new Type(LONG, null, ('J' << 24) | (1 << 16) | (1 << 8) | 2, 1);
public static readonly Type DOUBLE_TYPE = new Type(DOUBLE, null, ('D' << 24) | (3 << 16) | (3 << 8) | 2, 1);
private readonly int sort;
private readonly char[] buf;
private readonly int off;
private readonly int len;
private Type(int sort, char[] buf, int off, int len) {
this.sort = sort;
this.buf = buf;
this.off = off;
this.len = len;
}

public static Type getType(String typeDescriptor) {
return getType(typeDescriptor.toCharArray(), 0);
}
public static Type getObjectType(String internalName) {
char[] buf = internalName.toCharArray();
return new Type(buf[0] == '[' ? ARRAY : OBJECT, buf, 0, buf.length);
}
public static Type getMethodType(String methodDescriptor) {
return getType(methodDescriptor.toCharArray(), 0);
}
public static Type getMethodType(Type returnType, params Type[] argumentTypes) {
return getType(getMethodDescriptor(returnType, argumentTypes));
}
public static Type[] getArgumentTypes(String methodDescriptor) {
char[] buf = methodDescriptor.toCharArray();
int off = 1;
int size = 0;
while (true){
char car = buf[off++];
if (car == ')') {
break;
}
else if (car == 'L') {
while (buf[off++] != ';'){
}
++size;
}
else if (car != '[') {
++size;
}
}
Type[] args = new Type[size];
off = 1;
size = 0;
while (buf[off] != ')'){
args[size] = getType(buf, off);
off += args[size].len + (args[size].sort == OBJECT ? 2 : 0);
size += 1;
}
return args;
}
public static Type getReturnType(String methodDescriptor) {
char[] buf = methodDescriptor.toCharArray();
int off = 1;
while (true){
char car = buf[off++];
if (car == ')') {
return getType(buf, off);
}
else if (car == 'L') {
while (buf[off++] != ';'){
}
}
}
}
public static int getArgumentsAndReturnSizes(String desc) {
int n = 1;
int c = 1;
while (true){
char car = desc.charAt(c++);
if (car == ')') {
car = desc.charAt(c);
return n << 2 | (car == 'V' ? 0 : (car == 'D' || car == 'J' ? 2 : 1));
}
else if (car == 'L') {
while (desc.charAt(c++) != ';'){
}
n += 1;
}
else if (car == '[') {
while ((car = desc.charAt(c)) == '['){
++c;
}
if (car == 'D' || car == 'J') {
n -= 1;
}
}
else if (car == 'D' || car == 'J') {
n += 2;
}
else {
n += 1;
}
}
}
private static Type getType(char[] buf, int off) {
int len;
switch (buf[off]) {
case 'V':
return VOID_TYPE;
case 'Z':
return BOOLEAN_TYPE;
case 'C':
return CHAR_TYPE;
case 'B':
return BYTE_TYPE;
case 'S':
return SHORT_TYPE;
case 'I':
return INT_TYPE;
case 'F':
return FLOAT_TYPE;
case 'J':
return LONG_TYPE;
case 'D':
return DOUBLE_TYPE;
case '[':
len = 1;
while (buf[off + len] == '['){
++len;
}
if (buf[off + len] == 'L') {
++len;
while (buf[off + len] != ';'){
++len;
}
}
return new Type(ARRAY, buf, off, len + 1);
case 'L':
len = 1;
while (buf[off + len] != ';'){
++len;
}
return new Type(OBJECT, buf, off + 1, len - 1);
default:
return new Type(METHOD, buf, off, buf.length - off);
}
}
public virtual int getSort() {
return sort;
}
public virtual int getDimensions() {
int i = 1;
while (buf[off + i] == '['){
++i;
}
return i;
}
public virtual Type getElementType() {
return getType(buf, off + getDimensions());
}
public virtual String getClassName() {
switch (sort) {
case VOID:
return "void";
case BOOLEAN:
return "boolean";
case CHAR:
return "char";
case BYTE:
return "byte";
case SHORT:
return "short";
case INT:
return "int";
case FLOAT:
return "float";
case LONG:
return "long";
case DOUBLE:
return "double";
case ARRAY:
StringBuilder sb = new StringBuilder(getElementType().getClassName());
for (int i = getDimensions(); i > 0; --i) {
sb.append("[]");
}
return sb.toString();
case OBJECT:
return new String(buf, off, len).replace('/', '.');
default:
return null;
}
}
public virtual String getInternalName() {
return new String(buf, off, len);
}
public virtual Type[] getArgumentTypes() {
return getArgumentTypes(getDescriptor());
}
public virtual Type getReturnType() {
return getReturnType(getDescriptor());
}
public virtual int getArgumentsAndReturnSizes() {
return getArgumentsAndReturnSizes(getDescriptor());
}
public virtual String getDescriptor() {
StringBuilder buf = new StringBuilder();
getDescriptor(buf);
return buf.toString();
}
public static String getMethodDescriptor(Type returnType, params Type[] argumentTypes) {
StringBuilder buf = new StringBuilder();
buf.append('(');
for (int i = 0; i < argumentTypes.length; ++i) {
argumentTypes[i].getDescriptor(buf);
}
buf.append(')');
returnType.getDescriptor(buf);
return buf.toString();
}
private void getDescriptor(StringBuilder buf) {
if (this.buf == null) {
buf.append((char)((off & 0xFF000000) /*>>>*/ >> 24));
}
else if (sort == OBJECT) {
buf.append('L');
buf.append(this.buf, off, len);
buf.append(';');
}
else {
buf.append(this.buf, off, len);
}
}
public virtual int getSize() {
return buf == null ? (off & 0xFF) : 1;
}
public virtual int getOpcode(int opcode) {
if (opcode == Opcodes.IALOAD || opcode == Opcodes.IASTORE) {
return opcode + (buf == null ? (off & 0xFF00) >> 8 : 4);
}
else {
return opcode + (buf == null ? (off & 0xFF0000) >> 16 : 4);
}
}
public virtual bool Equals(Object o) {
if (this == o) {
return true;
}
if (!(o is Type)) {
return false;
}
Type t = (Type)o;
if (sort != t.sort) {
return false;
}
if (sort >= ARRAY) {
if (len != t.len) {
return false;
}
for (int i = off, j = t.off, end = i + len; i < end; i++, j++) {
if (buf[i] != t.buf[j]) {
return false;
}
}
}
return true;
}
public virtual int GetHashCode() {
int hc = 13 * sort;
if (sort >= ARRAY) {
for (int i = off, end = i + len; i < end; i++) {
hc = 17 * (hc + buf[i]);
}
}
return hc;
}
public virtual String ToString() {
return getDescriptor();
}
}
}
