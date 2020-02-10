using System;
using java.lang;

namespace org.objectweb.asm {
public class Attribute {
public readonly String type;
internal byte[] value;
internal Attribute next;
protected Attribute(String type) {
this.type = type;
}

public virtual bool isUnknown() {
return true;
}
public virtual bool isCodeAttribute() {
return false;
}
protected virtual Label[] getLabels() {
return null;
}
protected virtual Attribute read(ClassReader cr, int off, int len, char[] buf, int codeOff, Label[] labels) {
Attribute attr = new Attribute(type);
attr.value = new byte[len];
System.arraycopy(cr.b, off, attr.value, 0, len);
return attr;
}
protected virtual ByteVector write(ClassWriter cw, byte[] code, int len, int maxStack, int maxLocals) {
ByteVector v = new ByteVector();
v.data = value;
v.length = value.length;
return v;
}
internal int getCount() {
int count = 0;
Attribute attr = this;
while (attr != null){
count += 1;
attr = attr.next;
}
return count;
}
internal int getSize(ClassWriter cw, byte[] code, int len, int maxStack, int maxLocals) {
Attribute attr = this;
int size = 0;
while (attr != null){
cw.newUTF8(attr.type);
size += attr.write(cw, code, len, maxStack, maxLocals).length + 6;
attr = attr.next;
}
return size;
}
internal void put(ClassWriter cw, byte[] code, int len, int maxStack, int maxLocals, ByteVector @out) {
Attribute attr = this;
while (attr != null){
ByteVector b = attr.write(cw, code, len, maxStack, maxLocals);
@out.putShort(cw.newUTF8(attr.type)).putInt(b.length);
@out.putByteArray(b.data, 0, b.length);
attr = attr.next;
}
}
}
}
