using System;
using java.lang;

namespace org.objectweb.asm {
sealed internal class AnnotationWriter: AnnotationVisitor {
private readonly ClassWriter cw;
private int size;
private readonly bool named;
private readonly ByteVector bv;
private readonly ByteVector parent;
private readonly int offset;
internal AnnotationWriter next;
internal AnnotationWriter prev;
internal AnnotationWriter(ClassWriter cw, bool named, ByteVector bv, ByteVector parent, int offset): base(Opcodes.ASM5) {
this.cw = cw;
this.named = named;
this.bv = bv;
this.parent = parent;
this.offset = offset;
}

public void visit(String name, Object value) {
++size;
if (named) {
bv.putShort(cw.newUTF8(name));
}
if (value is String) {
bv.put12('s', cw.newUTF8((String)value));
}
else if (value is Byte) {
bv.put12('B', cw.newInteger(((Byte)value).byteValue()).index);
}
else if (value is Boolean) {
int v = ((Boolean)value).booleanValue() ? 1 : 0;
bv.put12('Z', cw.newInteger(v).index);
}
else if (value is Character) {
bv.put12('C', cw.newInteger(((Character)value).charValue()).index);
}
else if (value is Short) {
bv.put12('S', cw.newInteger(((Short)value).shortValue()).index);
}
else if (value is Type) {
bv.put12('c', cw.newUTF8(((Type)value).getDescriptor()));
}
else if (value is byte[]) {
byte[] v = (byte[])value;
bv.put12('[', v.Length);
for (int i = 0; i < v.Length; i++) {
bv.put12('B', cw.newInteger(v[i]).index);
}
}
else if (value is bool[]) {
bool[] v = (bool[])value;
bv.put12('[', v.Length);
for (int i = 0; i < v.Length; i++) {
bv.put12('Z', cw.newInteger(v[i] ? 1 : 0).index);
}
}
else if (value is short[]) {
short[] v = (short[])value;
bv.put12('[', v.Length);
for (int i = 0; i < v.Length; i++) {
bv.put12('S', cw.newInteger(v[i]).index);
}
}
else if (value is char[]) {
char[] v = (char[])value;
bv.put12('[', v.Length);
for (int i = 0; i < v.Length; i++) {
bv.put12('C', cw.newInteger(v[i]).index);
}
}
else if (value is int[]) {
int[] v = (int[])value;
bv.put12('[', v.Length);
for (int i = 0; i < v.Length; i++) {
bv.put12('I', cw.newInteger(v[i]).index);
}
}
else if (value is long[]) {
long[] v = (long[])value;
bv.put12('[', v.Length);
for (int i = 0; i < v.Length; i++) {
bv.put12('J', cw.newLong(v[i]).index);
}
}
else if (value is float[]) {
float[] v = (float[])value;
bv.put12('[', v.Length);
for (int i = 0; i < v.Length; i++) {
bv.put12('F', cw.newFloat(v[i]).index);
}
}
else if (value is double[]) {
double[] v = (double[])value;
bv.put12('[', v.Length);
for (int i = 0; i < v.Length; i++) {
bv.put12('D', cw.newDouble(v[i]).index);
}
}
else {
Item i = cw.newConstItem(value);
bv.put12(".s.IFJDCS".charAt(i.type), i.index);
}
}
public void visitEnum(String name, String desc, String value) {
++size;
if (named) {
bv.putShort(cw.newUTF8(name));
}
bv.put12('e', cw.newUTF8(desc)).putShort(cw.newUTF8(value));
}
public AnnotationVisitor visitAnnotation(String name, String desc) {
++size;
if (named) {
bv.putShort(cw.newUTF8(name));
}
bv.put12('@', cw.newUTF8(desc)).putShort(0);
return new AnnotationWriter(cw, true, bv, bv, bv.length - 2);
}
public AnnotationVisitor visitArray(String name) {
++size;
if (named) {
bv.putShort(cw.newUTF8(name));
}
bv.put12('[', 0);
return new AnnotationWriter(cw, false, bv, bv, bv.length - 2);
}
public void visitEnd() {
if (parent != null) {
byte[] data = parent.data;
data[offset] = (byte)(size /*>>>*/ >> 8);
data[offset + 1] = (byte)size;
}
}
internal int getSize() {
int size = 0;
AnnotationWriter aw = this;
while (aw != null){
size += aw.bv.length;
aw = aw.next;
}
return size;
}
internal void put(ByteVector @out) {
int n = 0;
int size = 2;
AnnotationWriter aw = this;
AnnotationWriter last = null;
while (aw != null){
++n;
size += aw.bv.length;
aw.visitEnd();
aw.prev = last;
last = aw;
aw = aw.next;
}
@out.putInt(size);
@out.putShort(n);
aw = last;
while (aw != null){
@out.putByteArray(aw.bv.data, 0, aw.bv.length);
aw = aw.prev;
}
}
static internal void put(AnnotationWriter[] panns, int off, ByteVector @out) {
int size = 1 + 2 * (panns.Length - off);
for (int i = off; i < panns.Length; ++i) {
size += panns[i] == null ? 0 : panns[i].getSize();
}
@out.putInt(size).putByte(panns.Length - off);
for (int i = off; i < panns.Length; ++i) {
AnnotationWriter aw = panns[i];
AnnotationWriter last = null;
int n = 0;
while (aw != null){
++n;
aw.visitEnd();
aw.prev = last;
last = aw;
aw = aw.next;
}
@out.putShort(n);
aw = last;
while (aw != null){
@out.putByteArray(aw.bv.data, 0, aw.bv.length);
aw = aw.prev;
}
}
}
static internal void putTarget(int typeRef, TypePath typePath, ByteVector @out) {
switch (typeRef /*>>>*/ >> 24) {
case 0x00:
case 0x01:
case 0x16:
@out.putShort(typeRef /*>>>*/ >> 16);
break;
case 0x13:
case 0x14:
case 0x15:
@out.putByte(typeRef /*>>>*/ >> 24);
break;
case 0x47:
case 0x48:
case 0x49:
case 0x4A:
case 0x4B:
@out.putInt(typeRef);
break;
default:
@out.put12(typeRef /*>>>*/ >> 24, (typeRef & 0xFFFF00) >> 8);
break;
}
if (typePath == null) {
@out.putByte(0);
}
else {
int length = typePath.b[typePath.offset] * 2 + 1;
@out.putByteArray(typePath.b, typePath.offset, length);
}
}
}
}
