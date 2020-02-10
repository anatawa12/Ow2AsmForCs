using System;
using java.lang;

namespace org.objectweb.asm {
public class Label {
internal const int DEBUG = 1;
internal const int RESOLVED = 2;
internal const int RESIZED = 4;
internal const int PUSHED = 8;
internal const int TARGET = 16;
internal const int STORE = 32;
internal const int REACHABLE = 64;
internal const int JSR = 128;
internal const int RET = 256;
internal const int SUBROUTINE = 512;
internal const int VISITED = 1024;
internal const int VISITED2 = 2048;
public Object info;
internal int status;
internal int line;
internal int position;
private int referenceCount;
private int[] srcAndRefPositions;
internal int inputStackTop;
internal int outputStackMax;
internal Frame frame;
internal Label successor;
internal Edge successors;
internal Label next;
public Label() {
}

public virtual int getOffset() {
if ((status & RESOLVED) == 0) {
throw new IllegalStateException("Label offset position has not been resolved yet");
}
return position;
}
internal virtual void put(MethodWriter owner, ByteVector @out, int source, bool wideOffset) {
if ((status & RESOLVED) == 0) {
if (wideOffset) {
addReference(-1 - source, @out.length);
@out.putInt(-1);
}
else {
addReference(source, @out.length);
@out.putShort(-1);
}
}
else {
if (wideOffset) {
@out.putInt(position - source);
}
else {
@out.putShort(position - source);
}
}
}
private void addReference(int sourcePosition, int referencePosition) {
if (srcAndRefPositions == null) {
srcAndRefPositions = new int[6];
}
if (referenceCount >= srcAndRefPositions.Length) {
int[] a = new int[srcAndRefPositions.Length + 6];
SystemJ.arraycopy(srcAndRefPositions, 0, a, 0, srcAndRefPositions.Length);
srcAndRefPositions = a;
}
srcAndRefPositions[referenceCount++] = sourcePosition;
srcAndRefPositions[referenceCount++] = referencePosition;
}
internal virtual bool resolve(MethodWriter owner, int position, byte[] data) {
bool needUpdate = false;
this.status |= RESOLVED;
this.position = position;
int i = 0;
while (i < referenceCount){
int source = srcAndRefPositions[i++];
int reference = srcAndRefPositions[i++];
int offset;
if (source >= 0) {
offset = position - source;
if (offset < Short.MIN_VALUE || offset > Short.MAX_VALUE) {
int opcode = data[reference - 1] & 0xFF;
if (opcode <= Opcodes.JSR) {
data[reference - 1] = (byte)(opcode + 49);
}
else {
data[reference - 1] = (byte)(opcode + 20);
}
needUpdate = true;
}
data[reference++] = (byte)(offset /*>>>*/ >> 8);
data[reference] = (byte)offset;
}
else {
offset = position + source + 1;
data[reference++] = (byte)(offset /*>>>*/ >> 24);
data[reference++] = (byte)(offset /*>>>*/ >> 16);
data[reference++] = (byte)(offset /*>>>*/ >> 8);
data[reference] = (byte)offset;
}
}
return needUpdate;
}
internal virtual Label getFirst() {
return !ClassReader.FRAMES || frame == null ? this : frame.owner;
}
internal virtual bool inSubroutine(long id) {
if ((status & Label.VISITED) != 0) {
return (srcAndRefPositions[(int)(id /*>>>*/ >> 32)] & (int)id) != 0;
}
return false;
}
internal virtual bool inSameSubroutine(Label block) {
if ((status & VISITED) == 0 || (block.status & VISITED) == 0) {
return false;
}
for (int i = 0; i < srcAndRefPositions.Length; ++i) {
if ((srcAndRefPositions[i] & block.srcAndRefPositions[i]) != 0) {
return true;
}
}
return false;
}
internal virtual void addToSubroutine(long id, int nbSubroutines) {
if ((status & VISITED) == 0) {
status |= VISITED;
srcAndRefPositions = new int[nbSubroutines / 32 + 1];
}
srcAndRefPositions[(int)(id /*>>>*/ >> 32)] |= (int)id;
}
internal virtual void visitSubroutine(Label JSR, long id, int nbSubroutines) {
Label stack = this;
while (stack != null){
Label l = stack;
stack = l.next;
l.next = null;
if (JSR != null) {
if ((l.status & VISITED2) != 0) {
continue;
}
l.status |= VISITED2;
if ((l.status & RET) != 0) {
if (!l.inSameSubroutine(JSR)) {
Edge e = new Edge();
e.info = l.inputStackTop;
e.successor = JSR.successors.successor;
e.next = l.successors;
l.successors = e;
}
}
}
else {
if (l.inSubroutine(id)) {
continue;
}
l.addToSubroutine(id, nbSubroutines);
}
Edge e = l.successors;
while (e != null){
if ((l.status & Label.JSR) == 0 || e != l.successors.next) {
if (e.successor.next == null) {
e.successor.next = stack;
stack = e.successor;
}
}
e = e.next;
}
}
}
public virtual String ToString() {
return "L" + SystemJ.identityHashCode(this);
}
}
}
