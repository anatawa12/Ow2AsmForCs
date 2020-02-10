using System;
using java.lang;

namespace org.objectweb.asm {
public class TypePath {
public const int ARRAY_ELEMENT = 0;
public const int INNER_TYPE = 1;
public const int WILDCARD_BOUND = 2;
public const int TYPE_ARGUMENT = 3;
internal byte[] b;
internal int offset;
internal TypePath(byte[] b, int offset) {
this.b = b;
this.offset = offset;
}

public virtual int getLength() {
return b[offset];
}
public virtual int getStep(int index) {
return b[offset + 2 * index + 1];
}
public virtual int getStepArgument(int index) {
return b[offset + 2 * index + 2];
}
public static TypePath fromString(String typePath) {
if (typePath == null || typePath.length() == 0) {
return null;
}
int n = typePath.length();
ByteVector @out = new ByteVector(n);
@out.putByte(0);
for (int i = 0; i < n; ) {
char c = typePath.charAt(i++);
if (c == '[') {
@out.put11(ARRAY_ELEMENT, 0);
}
else if (c == '.') {
@out.put11(INNER_TYPE, 0);
}
else if (c == '*') {
@out.put11(WILDCARD_BOUND, 0);
}
else if (c >= '0' && c <= '9') {
int typeArg = c - '0';
while (i < n && (c = typePath.charAt(i)) >= '0' && c <= '9'){
typeArg = typeArg * 10 + c - '0';
i += 1;
}
if (i < n && typePath.charAt(i) == ';') {
i += 1;
}
@out.put11(TYPE_ARGUMENT, typeArg);
}
}
@out.data[0] = (byte)(@out.length / 2);
return new TypePath(@out.data, 0);
}
public virtual String ToString() {
int length = getLength();
StringBuilder result = new StringBuilder(length * 2);
for (int i = 0; i < length; ++i) {
switch (getStep(i)) {
case ARRAY_ELEMENT:
result.append('[');
break;
case INNER_TYPE:
result.append('.');
break;
case WILDCARD_BOUND:
result.append('*');
break;
case TYPE_ARGUMENT:
result.append(getStepArgument(i)).append(';');
break;
default:
result.append('_');
}
}
return result.toString();
}
}
}
