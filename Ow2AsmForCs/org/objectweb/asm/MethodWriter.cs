using System;
using java.lang;

namespace org.objectweb.asm {
internal class MethodWriter: MethodVisitor {
internal const int ACC_CONSTRUCTOR = 0x80000;
internal const int SAME_FRAME = 0;
internal const int SAME_LOCALS_1_STACK_ITEM_FRAME = 64;
internal const int RESERVED = 128;
internal const int SAME_LOCALS_1_STACK_ITEM_FRAME_EXTENDED = 247;
internal const int CHOP_FRAME = 248;
internal const int SAME_FRAME_EXTENDED = 251;
internal const int APPEND_FRAME = 252;
internal const int FULL_FRAME = 255;
internal const int FRAMES = 0;
internal const int INSERTED_FRAMES = 1;
internal const int MAXS = 2;
internal const int NOTHING = 3;
readonly internal ClassWriter cw;
private int access;
private readonly int name;
private readonly int desc;
private readonly String descriptor;
internal String signature;
internal int classReaderOffset;
internal int classReaderLength;
internal int exceptionCount;
internal int[] exceptions;
private ByteVector annd;
private AnnotationWriter anns;
private AnnotationWriter ianns;
private AnnotationWriter tanns;
private AnnotationWriter itanns;
private AnnotationWriter[] panns;
private AnnotationWriter[] ipanns;
private int synthetics;
private Attribute attrs;
private ByteVector code = new ByteVector();
private int maxStack;
private int maxLocals;
private int currentLocals;
private int frameCount;
private ByteVector stackMap;
private int previousFrameOffset;
private int[] previousFrame;
private int[] frame;
private int handlerCount;
private Handler firstHandler;
private Handler lastHandler;
private int methodParametersCount;
private ByteVector methodParameters;
private int localVarCount;
private ByteVector localVar;
private int localVarTypeCount;
private ByteVector localVarType;
private int lineNumberCount;
private ByteVector lineNumber;
private int lastCodeOffset;
private AnnotationWriter ctanns;
private AnnotationWriter ictanns;
private Attribute cattrs;
private int subroutines;
private readonly int compute;
private Label labels;
private Label previousBlock;
private Label currentBlock;
private int stackSize;
private int maxStackSize;
internal MethodWriter(ClassWriter cw, int access, String name, String desc, String signature, String[] exceptions, int compute): base(Opcodes.ASM5) {
if (cw.firstMethod == null) {
cw.firstMethod = this;
}
else {
cw.lastMethod.mv = this;
}
cw.lastMethod = this;
this.cw = cw;
this.access = access;
if ("<init>".equals(name)) {
this.access |= ACC_CONSTRUCTOR;
}
this.name = cw.newUTF8(name);
this.desc = cw.newUTF8(desc);
this.descriptor = desc;
if (ClassReader.SIGNATURES) {
this.signature = signature;
}
if (exceptions != null && exceptions.Length > 0) {
exceptionCount = exceptions.Length;
this.exceptions = new int[exceptionCount];
for (int i = 0; i < exceptionCount; ++i) {
this.exceptions[i] = cw.newClass(exceptions[i]);
}
}
this.compute = compute;
if (compute != NOTHING) {
int size = Type.getArgumentsAndReturnSizes(descriptor) >> 2;
if ((access & Opcodes.ACC_STATIC) != 0) {
--size;
}
maxLocals = size;
currentLocals = size;
labels = new Label();
labels.status |= Label.PUSHED;
visitLabel(labels);
}
}

public virtual void visitParameter(String name, int access) {
if (methodParameters == null) {
methodParameters = new ByteVector();
}
++methodParametersCount;
methodParameters.putShort((name == null) ? 0 : cw.newUTF8(name)).putShort(access);
}
public virtual AnnotationVisitor visitAnnotationDefault() {
if (!ClassReader.ANNOTATIONS) {
return null;
}
annd = new ByteVector();
return new AnnotationWriter(cw, false, annd, null, 0);
}
public virtual AnnotationVisitor visitAnnotation(String desc, bool visible) {
if (!ClassReader.ANNOTATIONS) {
return null;
}
ByteVector bv = new ByteVector();
bv.putShort(cw.newUTF8(desc)).putShort(0);
AnnotationWriter aw = new AnnotationWriter(cw, true, bv, bv, 2);
if (visible) {
aw.next = anns;
anns = aw;
}
else {
aw.next = ianns;
ianns = aw;
}
return aw;
}
public virtual AnnotationVisitor visitTypeAnnotation(int typeRef, TypePath typePath, String desc, bool visible) {
if (!ClassReader.ANNOTATIONS) {
return null;
}
ByteVector bv = new ByteVector();
AnnotationWriter.putTarget(typeRef, typePath, bv);
bv.putShort(cw.newUTF8(desc)).putShort(0);
AnnotationWriter aw = new AnnotationWriter(cw, true, bv, bv, bv.length - 2);
if (visible) {
aw.next = tanns;
tanns = aw;
}
else {
aw.next = itanns;
itanns = aw;
}
return aw;
}
public virtual AnnotationVisitor visitParameterAnnotation(int parameter, String desc, bool visible) {
if (!ClassReader.ANNOTATIONS) {
return null;
}
ByteVector bv = new ByteVector();
if ("Ljava/lang/Synthetic;".equals(desc)) {
synthetics = Math.Max(synthetics, parameter + 1);
return new AnnotationWriter(cw, false, bv, null, 0);
}
bv.putShort(cw.newUTF8(desc)).putShort(0);
AnnotationWriter aw = new AnnotationWriter(cw, true, bv, bv, 2);
if (visible) {
if (panns == null) {
panns = new AnnotationWriter[Type.getArgumentTypes(descriptor).Length];
}
aw.next = panns[parameter];
panns[parameter] = aw;
}
else {
if (ipanns == null) {
ipanns = new AnnotationWriter[Type.getArgumentTypes(descriptor).Length];
}
aw.next = ipanns[parameter];
ipanns[parameter] = aw;
}
return aw;
}
public virtual void visitAttribute(Attribute attr) {
if (attr.isCodeAttribute()) {
attr.next = cattrs;
cattrs = attr;
}
else {
attr.next = attrs;
attrs = attr;
}
}
public virtual void visitCode() {
}
public virtual void visitFrame(int type, int nLocal, Object[] local, int nStack, Object[] stack) {
if (!ClassReader.FRAMES || compute == FRAMES) {
return;
}
if (compute == INSERTED_FRAMES) {
if (currentBlock.frame == null) {
currentBlock.frame = new CurrentFrame();
currentBlock.frame.owner = currentBlock;
currentBlock.frame.initInputFrame(cw, access, Type.getArgumentTypes(descriptor), nLocal);
visitImplicitFirstFrame();
}
else {
if (type == Opcodes.F_NEW) {
currentBlock.frame.set(cw, nLocal, local, nStack, stack);
}
else {
}
visitFrame(currentBlock.frame);
}
}
else if (type == Opcodes.F_NEW) {
if (previousFrame == null) {
visitImplicitFirstFrame();
}
currentLocals = nLocal;
int frameIndex = startFrame(code.length, nLocal, nStack);
for (int i = 0; i < nLocal; ++i) {
if (local[i] is String) {
frame[frameIndex++] = Frame.OBJECT | cw.addType((String)local[i]);
}
else if (local[i] is Integer) {
frame[frameIndex++] = ((Integer)local[i]).intValue();
}
else {
frame[frameIndex++] = Frame.UNINITIALIZED | cw.addUninitializedType("", ((Label)local[i]).position);
}
}
for (int i = 0; i < nStack; ++i) {
if (stack[i] is String) {
frame[frameIndex++] = Frame.OBJECT | cw.addType((String)stack[i]);
}
else if (stack[i] is Integer) {
frame[frameIndex++] = ((Integer)stack[i]).intValue();
}
else {
frame[frameIndex++] = Frame.UNINITIALIZED | cw.addUninitializedType("", ((Label)stack[i]).position);
}
}
endFrame();
}
else {
int delta;
if (stackMap == null) {
stackMap = new ByteVector();
delta = code.length;
}
else {
delta = code.length - previousFrameOffset - 1;
if (delta < 0) {
if (type == Opcodes.F_SAME) {
return;
}
else {
throw new IllegalStateException();
}
}
}
switch (type) {
case Opcodes.F_FULL:
currentLocals = nLocal;
stackMap.putByte(FULL_FRAME).putShort(delta).putShort(nLocal);
for (int i = 0; i < nLocal; ++i) {
writeFrameType(local[i]);
}
stackMap.putShort(nStack);
for (int i = 0; i < nStack; ++i) {
writeFrameType(stack[i]);
}
break;
case Opcodes.F_APPEND:
currentLocals += nLocal;
stackMap.putByte(SAME_FRAME_EXTENDED + nLocal).putShort(delta);
for (int i = 0; i < nLocal; ++i) {
writeFrameType(local[i]);
}
break;
case Opcodes.F_CHOP:
currentLocals -= nLocal;
stackMap.putByte(SAME_FRAME_EXTENDED - nLocal).putShort(delta);
break;
case Opcodes.F_SAME:
if (delta < 64) {
stackMap.putByte(delta);
}
else {
stackMap.putByte(SAME_FRAME_EXTENDED).putShort(delta);
}
break;
case Opcodes.F_SAME1:
if (delta < 64) {
stackMap.putByte(SAME_LOCALS_1_STACK_ITEM_FRAME + delta);
}
else {
stackMap.putByte(SAME_LOCALS_1_STACK_ITEM_FRAME_EXTENDED).putShort(delta);
}
writeFrameType(stack[0]);
break;
}
previousFrameOffset = code.length;
++frameCount;
}
maxStack = Math.Max(maxStack, nStack);
maxLocals = Math.Max(maxLocals, currentLocals);
}
public virtual void visitInsn(int opcode) {
lastCodeOffset = code.length;
code.putByte(opcode);
if (currentBlock != null) {
if (compute == FRAMES || compute == INSERTED_FRAMES) {
currentBlock.frame.execute(opcode, 0, null, null);
}
else {
int size = stackSize + Frame.SIZE[opcode];
if (size > maxStackSize) {
maxStackSize = size;
}
stackSize = size;
}
if ((opcode >= Opcodes.IRETURN && opcode <= Opcodes.RETURN) || opcode == Opcodes.ATHROW) {
noSuccessor();
}
}
}
public virtual void visitIntInsn(int opcode, int operand) {
lastCodeOffset = code.length;
if (currentBlock != null) {
if (compute == FRAMES || compute == INSERTED_FRAMES) {
currentBlock.frame.execute(opcode, operand, null, null);
}
else if (opcode != Opcodes.NEWARRAY) {
int size = stackSize + 1;
if (size > maxStackSize) {
maxStackSize = size;
}
stackSize = size;
}
}
if (opcode == Opcodes.SIPUSH) {
code.put12(opcode, operand);
}
else {
code.put11(opcode, operand);
}
}
public virtual void visitVarInsn(int opcode, int var) {
lastCodeOffset = code.length;
if (currentBlock != null) {
if (compute == FRAMES || compute == INSERTED_FRAMES) {
currentBlock.frame.execute(opcode, var, null, null);
}
else {
if (opcode == Opcodes.RET) {
currentBlock.status |= Label.RET;
currentBlock.inputStackTop = stackSize;
noSuccessor();
}
else {
int size = stackSize + Frame.SIZE[opcode];
if (size > maxStackSize) {
maxStackSize = size;
}
stackSize = size;
}
}
}
if (compute != NOTHING) {
int n;
if (opcode == Opcodes.LLOAD || opcode == Opcodes.DLOAD || opcode == Opcodes.LSTORE || opcode == Opcodes.DSTORE) {
n = var + 2;
}
else {
n = var + 1;
}
if (n > maxLocals) {
maxLocals = n;
}
}
if (var < 4 && opcode != Opcodes.RET) {
int opt;
if (opcode < Opcodes.ISTORE) {
opt = 26 + ((opcode - Opcodes.ILOAD) << 2) + var;
}
else {
opt = 59 + ((opcode - Opcodes.ISTORE) << 2) + var;
}
code.putByte(opt);
}
else if (var >= 256) {
code.putByte(196).put12(opcode, var);
}
else {
code.put11(opcode, var);
}
if (opcode >= Opcodes.ISTORE && compute == FRAMES && handlerCount > 0) {
visitLabel(new Label());
}
}
public virtual void visitTypeInsn(int opcode, String type) {
lastCodeOffset = code.length;
Item i = cw.newClassItem(type);
if (currentBlock != null) {
if (compute == FRAMES || compute == INSERTED_FRAMES) {
currentBlock.frame.execute(opcode, code.length, cw, i);
}
else if (opcode == Opcodes.NEW) {
int size = stackSize + 1;
if (size > maxStackSize) {
maxStackSize = size;
}
stackSize = size;
}
}
code.put12(opcode, i.index);
}
public virtual void visitFieldInsn(int opcode, String owner, String name, String desc) {
lastCodeOffset = code.length;
Item i = cw.newFieldItem(owner, name, desc);
if (currentBlock != null) {
if (compute == FRAMES || compute == INSERTED_FRAMES) {
currentBlock.frame.execute(opcode, 0, cw, i);
}
else {
int size;
char c = desc.charAt(0);
switch (opcode) {
case Opcodes.GETSTATIC:
size = stackSize + (c == 'D' || c == 'J' ? 2 : 1);
break;
case Opcodes.PUTSTATIC:
size = stackSize + (c == 'D' || c == 'J' ? -2 : -1);
break;
case Opcodes.GETFIELD:
size = stackSize + (c == 'D' || c == 'J' ? 1 : 0);
break;
default:
size = stackSize + (c == 'D' || c == 'J' ? -3 : -2);
break;
}
if (size > maxStackSize) {
maxStackSize = size;
}
stackSize = size;
}
}
code.put12(opcode, i.index);
}
public virtual void visitMethodInsn(int opcode, String owner, String name, String desc, bool itf) {
lastCodeOffset = code.length;
Item i = cw.newMethodItem(owner, name, desc, itf);
int argSize = i.intVal;
if (currentBlock != null) {
if (compute == FRAMES || compute == INSERTED_FRAMES) {
currentBlock.frame.execute(opcode, 0, cw, i);
}
else {
if (argSize == 0) {
argSize = Type.getArgumentsAndReturnSizes(desc);
i.intVal = argSize;
}
int size;
if (opcode == Opcodes.INVOKESTATIC) {
size = stackSize - (argSize >> 2) + (argSize & 0x03) + 1;
}
else {
size = stackSize - (argSize >> 2) + (argSize & 0x03);
}
if (size > maxStackSize) {
maxStackSize = size;
}
stackSize = size;
}
}
if (opcode == Opcodes.INVOKEINTERFACE) {
if (argSize == 0) {
argSize = Type.getArgumentsAndReturnSizes(desc);
i.intVal = argSize;
}
code.put12(Opcodes.INVOKEINTERFACE, i.index).put11(argSize >> 2, 0);
}
else {
code.put12(opcode, i.index);
}
}
public virtual void visitInvokeDynamicInsn(String name, String desc, Handle bsm, params Object[] bsmArgs) {
lastCodeOffset = code.length;
Item i = cw.newInvokeDynamicItem(name, desc, bsm, bsmArgs);
int argSize = i.intVal;
if (currentBlock != null) {
if (compute == FRAMES || compute == INSERTED_FRAMES) {
currentBlock.frame.execute(Opcodes.INVOKEDYNAMIC, 0, cw, i);
}
else {
if (argSize == 0) {
argSize = Type.getArgumentsAndReturnSizes(desc);
i.intVal = argSize;
}
int size = stackSize - (argSize >> 2) + (argSize & 0x03) + 1;
if (size > maxStackSize) {
maxStackSize = size;
}
stackSize = size;
}
}
code.put12(Opcodes.INVOKEDYNAMIC, i.index);
code.putShort(0);
}
public virtual void visitJumpInsn(int opcode, Label label) {
bool isWide = opcode >= 200;
opcode = isWide ? opcode - 33 : opcode;
lastCodeOffset = code.length;
Label nextInsn = null;
if (currentBlock != null) {
if (compute == FRAMES) {
currentBlock.frame.execute(opcode, 0, null, null);
label.getFirst().status |= Label.TARGET;
addSuccessor(Edge.NORMAL, label);
if (opcode != Opcodes.GOTO) {
nextInsn = new Label();
}
}
else if (compute == INSERTED_FRAMES) {
currentBlock.frame.execute(opcode, 0, null, null);
}
else {
if (opcode == Opcodes.JSR) {
if ((label.status & Label.SUBROUTINE) == 0) {
label.status |= Label.SUBROUTINE;
++subroutines;
}
currentBlock.status |= Label.JSR;
addSuccessor(stackSize + 1, label);
nextInsn = new Label();
}
else {
stackSize += Frame.SIZE[opcode];
addSuccessor(stackSize, label);
}
}
}
if ((label.status & Label.RESOLVED) != 0 && label.position - code.length < short.MinValue) {
if (opcode == Opcodes.GOTO) {
code.putByte(200);
}
else if (opcode == Opcodes.JSR) {
code.putByte(201);
}
else {
if (nextInsn != null) {
nextInsn.status |= Label.TARGET;
}
code.putByte(opcode <= 166 ? ((opcode + 1) ^ 1) - 1 : opcode ^ 1);
code.putShort(8);
code.putByte(200);
}
label.put(this, code, code.length - 1, true);
}
else if (isWide) {
code.putByte(opcode + 33);
label.put(this, code, code.length - 1, true);
}
else {
code.putByte(opcode);
label.put(this, code, code.length - 1, false);
}
if (currentBlock != null) {
if (nextInsn != null) {
visitLabel(nextInsn);
}
if (opcode == Opcodes.GOTO) {
noSuccessor();
}
}
}
public virtual void visitLabel(Label label) {
cw.hasAsmInsns |= label.resolve(this, code.length, code.data);
if ((label.status & Label.DEBUG) != 0) {
return;
}
if (compute == FRAMES) {
if (currentBlock != null) {
if (label.position == currentBlock.position) {
currentBlock.status |= (label.status & Label.TARGET);
label.frame = currentBlock.frame;
return;
}
addSuccessor(Edge.NORMAL, label);
}
currentBlock = label;
if (label.frame == null) {
label.frame = new Frame();
label.frame.owner = label;
}
if (previousBlock != null) {
if (label.position == previousBlock.position) {
previousBlock.status |= (label.status & Label.TARGET);
label.frame = previousBlock.frame;
currentBlock = previousBlock;
return;
}
previousBlock.successor = label;
}
previousBlock = label;
}
else if (compute == INSERTED_FRAMES) {
if (currentBlock == null) {
currentBlock = label;
}
else {
currentBlock.frame.owner = label;
}
}
else if (compute == MAXS) {
if (currentBlock != null) {
currentBlock.outputStackMax = maxStackSize;
addSuccessor(stackSize, label);
}
currentBlock = label;
stackSize = 0;
maxStackSize = 0;
if (previousBlock != null) {
previousBlock.successor = label;
}
previousBlock = label;
}
}
public virtual void visitLdcInsn(Object cst) {
lastCodeOffset = code.length;
Item i = cw.newConstItem(cst);
if (currentBlock != null) {
if (compute == FRAMES || compute == INSERTED_FRAMES) {
currentBlock.frame.execute(Opcodes.LDC, 0, cw, i);
}
else {
int size;
if (i.type == ClassWriter.LONG || i.type == ClassWriter.DOUBLE) {
size = stackSize + 2;
}
else {
size = stackSize + 1;
}
if (size > maxStackSize) {
maxStackSize = size;
}
stackSize = size;
}
}
int index = i.index;
if (i.type == ClassWriter.LONG || i.type == ClassWriter.DOUBLE) {
code.put12(20, index);
}
else if (index >= 256) {
code.put12(19, index);
}
else {
code.put11(Opcodes.LDC, index);
}
}
public virtual void visitIincInsn(int var, int increment) {
lastCodeOffset = code.length;
if (currentBlock != null) {
if (compute == FRAMES || compute == INSERTED_FRAMES) {
currentBlock.frame.execute(Opcodes.IINC, var, null, null);
}
}
if (compute != NOTHING) {
int n = var + 1;
if (n > maxLocals) {
maxLocals = n;
}
}
if ((var > 255) || (increment > 127) || (increment < -128)) {
code.putByte(196).put12(Opcodes.IINC, var).putShort(increment);
}
else {
code.putByte(Opcodes.IINC).put11(var, increment);
}
}
public virtual void visitTableSwitchInsn(int min, int max, Label dflt, params Label[] labels) {
lastCodeOffset = code.length;
int source = code.length;
code.putByte(Opcodes.TABLESWITCH);
code.putByteArray(null, 0, (4 - code.length % 4) % 4);
dflt.put(this, code, source, true);
code.putInt(min).putInt(max);
for (int i = 0; i < labels.Length; ++i) {
labels[i].put(this, code, source, true);
}
visitSwitchInsn(dflt, labels);
}
public virtual void visitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels) {
lastCodeOffset = code.length;
int source = code.length;
code.putByte(Opcodes.LOOKUPSWITCH);
code.putByteArray(null, 0, (4 - code.length % 4) % 4);
dflt.put(this, code, source, true);
code.putInt(labels.Length);
for (int i = 0; i < labels.Length; ++i) {
code.putInt(keys[i]);
labels[i].put(this, code, source, true);
}
visitSwitchInsn(dflt, labels);
}
private void visitSwitchInsn(Label dflt, Label[] labels) {
if (currentBlock != null) {
if (compute == FRAMES) {
currentBlock.frame.execute(Opcodes.LOOKUPSWITCH, 0, null, null);
addSuccessor(Edge.NORMAL, dflt);
dflt.getFirst().status |= Label.TARGET;
for (int i = 0; i < labels.Length; ++i) {
addSuccessor(Edge.NORMAL, labels[i]);
labels[i].getFirst().status |= Label.TARGET;
}
}
else {
--stackSize;
addSuccessor(stackSize, dflt);
for (int i = 0; i < labels.Length; ++i) {
addSuccessor(stackSize, labels[i]);
}
}
noSuccessor();
}
}
public virtual void visitMultiANewArrayInsn(String desc, int dims) {
lastCodeOffset = code.length;
Item i = cw.newClassItem(desc);
if (currentBlock != null) {
if (compute == FRAMES || compute == INSERTED_FRAMES) {
currentBlock.frame.execute(Opcodes.MULTIANEWARRAY, dims, cw, i);
}
else {
stackSize += 1 - dims;
}
}
code.put12(Opcodes.MULTIANEWARRAY, i.index).putByte(dims);
}
public virtual AnnotationVisitor visitInsnAnnotation(int typeRef, TypePath typePath, String desc, bool visible) {
if (!ClassReader.ANNOTATIONS) {
return null;
}
ByteVector bv = new ByteVector();
typeRef = (typeRef & 0xFF0000FF) | (lastCodeOffset << 8);
AnnotationWriter.putTarget(typeRef, typePath, bv);
bv.putShort(cw.newUTF8(desc)).putShort(0);
AnnotationWriter aw = new AnnotationWriter(cw, true, bv, bv, bv.length - 2);
if (visible) {
aw.next = ctanns;
ctanns = aw;
}
else {
aw.next = ictanns;
ictanns = aw;
}
return aw;
}
public virtual void visitTryCatchBlock(Label start, Label end, Label handler, String type) {
++handlerCount;
Handler h = new Handler();
h.start = start;
h.end = end;
h.handler = handler;
h.desc = type;
h.type = type != null ? cw.newClass(type) : 0;
if (lastHandler == null) {
firstHandler = h;
}
else {
lastHandler.next = h;
}
lastHandler = h;
}
public virtual AnnotationVisitor visitTryCatchAnnotation(int typeRef, TypePath typePath, String desc, bool visible) {
if (!ClassReader.ANNOTATIONS) {
return null;
}
ByteVector bv = new ByteVector();
AnnotationWriter.putTarget(typeRef, typePath, bv);
bv.putShort(cw.newUTF8(desc)).putShort(0);
AnnotationWriter aw = new AnnotationWriter(cw, true, bv, bv, bv.length - 2);
if (visible) {
aw.next = ctanns;
ctanns = aw;
}
else {
aw.next = ictanns;
ictanns = aw;
}
return aw;
}
public virtual void visitLocalVariable(String name, String desc, String signature, Label start, Label end, int index) {
if (signature != null) {
if (localVarType == null) {
localVarType = new ByteVector();
}
++localVarTypeCount;
localVarType.putShort(start.position).putShort(end.position - start.position).putShort(cw.newUTF8(name)).putShort(cw.newUTF8(signature)).putShort(index);
}
if (localVar == null) {
localVar = new ByteVector();
}
++localVarCount;
localVar.putShort(start.position).putShort(end.position - start.position).putShort(cw.newUTF8(name)).putShort(cw.newUTF8(desc)).putShort(index);
if (compute != NOTHING) {
char c = desc.charAt(0);
int n = index + (c == 'J' || c == 'D' ? 2 : 1);
if (n > maxLocals) {
maxLocals = n;
}
}
}
public virtual AnnotationVisitor visitLocalVariableAnnotation(int typeRef, TypePath typePath, Label[] start, Label[] end, int[] index, String desc, bool visible) {
if (!ClassReader.ANNOTATIONS) {
return null;
}
ByteVector bv = new ByteVector();
bv.putByte(typeRef /*>>>*/ >> 24).putShort(start.Length);
for (int i = 0; i < start.Length; ++i) {
bv.putShort(start[i].position).putShort(end[i].position - start[i].position).putShort(index[i]);
}
if (typePath == null) {
bv.putByte(0);
}
else {
int length = typePath.b[typePath.offset] * 2 + 1;
bv.putByteArray(typePath.b, typePath.offset, length);
}
bv.putShort(cw.newUTF8(desc)).putShort(0);
AnnotationWriter aw = new AnnotationWriter(cw, true, bv, bv, bv.length - 2);
if (visible) {
aw.next = ctanns;
ctanns = aw;
}
else {
aw.next = ictanns;
ictanns = aw;
}
return aw;
}
public virtual void visitLineNumber(int line, Label start) {
if (lineNumber == null) {
lineNumber = new ByteVector();
}
++lineNumberCount;
lineNumber.putShort(start.position);
lineNumber.putShort(line);
}
public virtual void visitMaxs(int maxStack, int maxLocals) {
if (ClassReader.FRAMES && compute == FRAMES) {
Handler handler = firstHandler;
while (handler != null){
Label l = handler.start.getFirst();
Label h = handler.handler.getFirst();
Label e = handler.end.getFirst();
String t = handler.desc == null ? "java/lang/Throwable" : handler.desc;
int kind = Frame.OBJECT | cw.addType(t);
h.status |= Label.TARGET;
while (l != e){
Edge b = new Edge();
b.info = kind;
b.successor = h;
b.next = l.successors;
l.successors = b;
l = l.successor;
}
handler = handler.next;
}
Frame f = labels.frame;
f.initInputFrame(cw, access, Type.getArgumentTypes(descriptor), this.maxLocals);
visitFrame(f);
int max = 0;
Label changed = labels;
while (changed != null){
Label l = changed;
changed = changed.next;
l.next = null;
f = l.frame;
if ((l.status & Label.TARGET) != 0) {
l.status |= Label.STORE;
}
l.status |= Label.REACHABLE;
int blockMax = f.inputStack.Length + l.outputStackMax;
if (blockMax > max) {
max = blockMax;
}
Edge e = l.successors;
while (e != null){
Label n = e.successor.getFirst();
bool change = f.merge(cw, n.frame, e.info);
if (change && n.next == null) {
n.next = changed;
changed = n;
}
e = e.next;
}
}
{
Label l = labels;
while (l != null){
f = l.frame;
if ((l.status & Label.STORE) != 0) {
visitFrame(f);
}
if ((l.status & Label.REACHABLE) == 0) {
Label k = l.successor;
int start = l.position;
int end = (k == null ? code.length : k.position) - 1;
if (end >= start) {
max = Math.Max(max, 1);
for (int i = start; i < end; ++i) {
code.data[i] = Opcodes.NOP;
}
code.data[end] = (byte)Opcodes.ATHROW;
int frameIndex = startFrame(start, 0, 1);
frame[frameIndex] = Frame.OBJECT | cw.addType("java/lang/Throwable");
endFrame();
firstHandler = Handler.remove(firstHandler, l, k);
}
}
l = l.successor;
}
handler = firstHandler;
handlerCount = 0;
while (handler != null){
handlerCount += 1;
handler = handler.next;
}
this.maxStack = max;
}
}
else if (compute == MAXS) {
Handler handler = firstHandler;
while (handler != null){
Label l = handler.start;
Label h = handler.handler;
Label e = handler.end;
while (l != e){
Edge b = new Edge();
b.info = Edge.EXCEPTION;
b.successor = h;
if ((l.status & Label.JSR) == 0) {
b.next = l.successors;
l.successors = b;
}
else {
b.next = l.successors.next.next;
l.successors.next.next = b;
}
l = l.successor;
}
handler = handler.next;
}
if (subroutines > 0) {
int id = 0;
labels.visitSubroutine(null, 1, subroutines);
Label l = labels;
while (l != null){
if ((l.status & Label.JSR) != 0) {
Label subroutine = l.successors.next.successor;
if ((subroutine.status & Label.VISITED) == 0) {
id += 1;
subroutine.visitSubroutine(null, (id / 32L) << 32 | (1L << (id % 32)), subroutines);
}
}
l = l.successor;
}
l = labels;
while (l != null){
if ((l.status & Label.JSR) != 0) {
Label L = labels;
while (L != null){
L.status &= ~Label.VISITED2;
L = L.successor;
}
Label subroutine = l.successors.next.successor;
subroutine.visitSubroutine(l, 0, subroutines);
}
l = l.successor;
}
}
int max = 0;
Label stack = labels;
while (stack != null){
Label l = stack;
stack = stack.next;
int start = l.inputStackTop;
int blockMax = start + l.outputStackMax;
if (blockMax > max) {
max = blockMax;
}
Edge b = l.successors;
if ((l.status & Label.JSR) != 0) {
b = b.next;
}
while (b != null){
l = b.successor;
if ((l.status & Label.PUSHED) == 0) {
l.inputStackTop = b.info == Edge.EXCEPTION ? 1 : start + b.info;
l.status |= Label.PUSHED;
l.next = stack;
stack = l;
}
b = b.next;
}
}
this.maxStack = Math.Max(maxStack, max);
}
else {
this.maxStack = maxStack;
this.maxLocals = maxLocals;
}
}
public virtual void visitEnd() {
}
private void addSuccessor(int info, Label successor) {
Edge b = new Edge();
b.info = info;
b.successor = successor;
b.next = currentBlock.successors;
currentBlock.successors = b;
}
private void noSuccessor() {
if (compute == FRAMES) {
Label l = new Label();
l.frame = new Frame();
l.frame.owner = l;
l.resolve(this, code.length, code.data);
previousBlock.successor = l;
previousBlock = l;
}
else {
currentBlock.outputStackMax = maxStackSize;
}
if (compute != INSERTED_FRAMES) {
currentBlock = null;
}
}
private void visitFrame(Frame f) {
int i, t;
int nTop = 0;
int nLocal = 0;
int nStack = 0;
int[] locals = f.inputLocals;
int[] stacks = f.inputStack;
for (i = 0; i < locals.Length; ++i) {
t = locals[i];
if (t == Frame.TOP) {
++nTop;
}
else {
nLocal += nTop + 1;
nTop = 0;
}
if (t == Frame.LONG || t == Frame.DOUBLE) {
++i;
}
}
for (i = 0; i < stacks.Length; ++i) {
t = stacks[i];
++nStack;
if (t == Frame.LONG || t == Frame.DOUBLE) {
++i;
}
}
int frameIndex = startFrame(f.owner.position, nLocal, nStack);
for (i = 0; nLocal > 0; ++i, --nLocal) {
t = locals[i];
frame[frameIndex++] = t;
if (t == Frame.LONG || t == Frame.DOUBLE) {
++i;
}
}
for (i = 0; i < stacks.Length; ++i) {
t = stacks[i];
frame[frameIndex++] = t;
if (t == Frame.LONG || t == Frame.DOUBLE) {
++i;
}
}
endFrame();
}
private void visitImplicitFirstFrame() {
int frameIndex = startFrame(0, descriptor.length() + 1, 0);
if ((access & Opcodes.ACC_STATIC) == 0) {
if ((access & ACC_CONSTRUCTOR) == 0) {
frame[frameIndex++] = Frame.OBJECT | cw.addType(cw.thisName);
}
else {
frame[frameIndex++] = 6;
}
}
int i = 1;
loop:
while (true){
int j = i;
switch (descriptor.charAt(i++)) {
case 'Z':
case 'C':
case 'B':
case 'S':
case 'I':
frame[frameIndex++] = 1;
break;
case 'F':
frame[frameIndex++] = 2;
break;
case 'J':
frame[frameIndex++] = 4;
break;
case 'D':
frame[frameIndex++] = 3;
break;
case '[':
while (descriptor.charAt(i) == '['){
++i;
}
if (descriptor.charAt(i) == 'L') {
++i;
while (descriptor.charAt(i) != ';'){
++i;
}
}
frame[frameIndex++] = Frame.OBJECT | cw.addType(descriptor.substring(j, ++i));
break;
case 'L':
while (descriptor.charAt(i) != ';'){
++i;
}
frame[frameIndex++] = Frame.OBJECT | cw.addType(descriptor.substring(j + 1, i++));
break;
default:
break /* label: loop */;
}
}
frame[1] = frameIndex - 3;
endFrame();
}
private int startFrame(int offset, int nLocal, int nStack) {
int n = 3 + nLocal + nStack;
if (frame == null || frame.length < n) {
frame = new int[n];
}
frame[0] = offset;
frame[1] = nLocal;
frame[2] = nStack;
return 3;
}
private void endFrame() {
if (previousFrame != null) {
if (stackMap == null) {
stackMap = new ByteVector();
}
writeFrame();
++frameCount;
}
previousFrame = frame;
frame = null;
}
private void writeFrame() {
int clocalsSize = frame[1];
int cstackSize = frame[2];
if ((cw.version & 0xFFFF) < Opcodes.V1_6) {
stackMap.putShort(frame[0]).putShort(clocalsSize);
writeFrameTypes(3, 3 + clocalsSize);
stackMap.putShort(cstackSize);
writeFrameTypes(3 + clocalsSize, 3 + clocalsSize + cstackSize);
return;
}
int localsSize = previousFrame[1];
int type = FULL_FRAME;
int k = 0;
int delta;
if (frameCount == 0) {
delta = frame[0];
}
else {
delta = frame[0] - previousFrame[0] - 1;
}
if (cstackSize == 0) {
k = clocalsSize - localsSize;
switch (k) {
case -3:
case -2:
case -1:
type = CHOP_FRAME;
localsSize = clocalsSize;
break;
case 0:
type = delta < 64 ? SAME_FRAME : SAME_FRAME_EXTENDED;
break;
case 1:
case 2:
case 3:
type = APPEND_FRAME;
break;
}
}
else if (clocalsSize == localsSize && cstackSize == 1) {
type = delta < 63 ? SAME_LOCALS_1_STACK_ITEM_FRAME : SAME_LOCALS_1_STACK_ITEM_FRAME_EXTENDED;
}
if (type != FULL_FRAME) {
int l = 3;
for (int j = 0; j < localsSize; j++) {
if (frame[l] != previousFrame[l]) {
type = FULL_FRAME;
break;
}
l++;
}
}
switch (type) {
case SAME_FRAME:
stackMap.putByte(delta);
break;
case SAME_LOCALS_1_STACK_ITEM_FRAME:
stackMap.putByte(SAME_LOCALS_1_STACK_ITEM_FRAME + delta);
writeFrameTypes(3 + clocalsSize, 4 + clocalsSize);
break;
case SAME_LOCALS_1_STACK_ITEM_FRAME_EXTENDED:
stackMap.putByte(SAME_LOCALS_1_STACK_ITEM_FRAME_EXTENDED).putShort(delta);
writeFrameTypes(3 + clocalsSize, 4 + clocalsSize);
break;
case SAME_FRAME_EXTENDED:
stackMap.putByte(SAME_FRAME_EXTENDED).putShort(delta);
break;
case CHOP_FRAME:
stackMap.putByte(SAME_FRAME_EXTENDED + k).putShort(delta);
break;
case APPEND_FRAME:
stackMap.putByte(SAME_FRAME_EXTENDED + k).putShort(delta);
writeFrameTypes(3 + localsSize, 3 + clocalsSize);
break;
default:
stackMap.putByte(FULL_FRAME).putShort(delta).putShort(clocalsSize);
writeFrameTypes(3, 3 + clocalsSize);
stackMap.putShort(cstackSize);
writeFrameTypes(3 + clocalsSize, 3 + clocalsSize + cstackSize);
}
}
private void writeFrameTypes(int start, int end) {
for (int i = start; i < end; ++i) {
int t = frame[i];
int d = t & Frame.DIM;
if (d == 0) {
int v = t & Frame.BASE_VALUE;
switch (t & Frame.BASE_KIND) {
case Frame.OBJECT:
stackMap.putByte(7).putShort(cw.newClass(cw.typeTable[v].strVal1));
break;
case Frame.UNINITIALIZED:
stackMap.putByte(8).putShort(cw.typeTable[v].intVal);
break;
default:
stackMap.putByte(v);
}
}
else {
StringBuilder sb = new StringBuilder();
d >>= 28;
while (d-- > 0){
sb.append('[');
}
if ((t & Frame.BASE_KIND) == Frame.OBJECT) {
sb.append('L');
sb.append(cw.typeTable[t & Frame.BASE_VALUE].strVal1);
sb.append(';');
}
else {
switch (t & 0xF) {
case 1:
sb.append('I');
break;
case 2:
sb.append('F');
break;
case 3:
sb.append('D');
break;
case 9:
sb.append('Z');
break;
case 10:
sb.append('B');
break;
case 11:
sb.append('C');
break;
case 12:
sb.append('S');
break;
default:
sb.append('J');
}
}
stackMap.putByte(7).putShort(cw.newClass(sb.toString()));
}
}
}
private void writeFrameType(Object type) {
if (type is String) {
stackMap.putByte(7).putShort(cw.newClass((String)type));
}
else if (type is Integer) {
stackMap.putByte(((Integer)type).intValue());
}
else {
stackMap.putByte(8).putShort(((Label)type).position);
}
}
internal int getSize() {
if (classReaderOffset != 0) {
return 6 + classReaderLength;
}
int size = 8;
if (code.length > 0) {
if (code.length > 65535) {
throw new RuntimeException("Method code too large!");
}
cw.newUTF8("Code");
size += 18 + code.length + 8 * handlerCount;
if (localVar != null) {
cw.newUTF8("LocalVariableTable");
size += 8 + localVar.length;
}
if (localVarType != null) {
cw.newUTF8("LocalVariableTypeTable");
size += 8 + localVarType.length;
}
if (lineNumber != null) {
cw.newUTF8("LineNumberTable");
size += 8 + lineNumber.length;
}
if (stackMap != null) {
bool zip = (cw.version & 0xFFFF) >= Opcodes.V1_6;
cw.newUTF8(zip ? "StackMapTable" : "StackMap");
size += 8 + stackMap.length;
}
if (ClassReader.ANNOTATIONS && ctanns != null) {
cw.newUTF8("RuntimeVisibleTypeAnnotations");
size += 8 + ctanns.getSize();
}
if (ClassReader.ANNOTATIONS && ictanns != null) {
cw.newUTF8("RuntimeInvisibleTypeAnnotations");
size += 8 + ictanns.getSize();
}
if (cattrs != null) {
size += cattrs.getSize(cw, code.data, code.length, maxStack, maxLocals);
}
}
if (exceptionCount > 0) {
cw.newUTF8("Exceptions");
size += 8 + 2 * exceptionCount;
}
if ((access & Opcodes.ACC_SYNTHETIC) != 0) {
if ((cw.version & 0xFFFF) < Opcodes.V1_5 || (access & ClassWriter.ACC_SYNTHETIC_ATTRIBUTE) != 0) {
cw.newUTF8("Synthetic");
size += 6;
}
}
if ((access & Opcodes.ACC_DEPRECATED) != 0) {
cw.newUTF8("Deprecated");
size += 6;
}
if (ClassReader.SIGNATURES && signature != null) {
cw.newUTF8("Signature");
cw.newUTF8(signature);
size += 8;
}
if (methodParameters != null) {
cw.newUTF8("MethodParameters");
size += 7 + methodParameters.length;
}
if (ClassReader.ANNOTATIONS && annd != null) {
cw.newUTF8("AnnotationDefault");
size += 6 + annd.length;
}
if (ClassReader.ANNOTATIONS && anns != null) {
cw.newUTF8("RuntimeVisibleAnnotations");
size += 8 + anns.getSize();
}
if (ClassReader.ANNOTATIONS && ianns != null) {
cw.newUTF8("RuntimeInvisibleAnnotations");
size += 8 + ianns.getSize();
}
if (ClassReader.ANNOTATIONS && tanns != null) {
cw.newUTF8("RuntimeVisibleTypeAnnotations");
size += 8 + tanns.getSize();
}
if (ClassReader.ANNOTATIONS && itanns != null) {
cw.newUTF8("RuntimeInvisibleTypeAnnotations");
size += 8 + itanns.getSize();
}
if (ClassReader.ANNOTATIONS && panns != null) {
cw.newUTF8("RuntimeVisibleParameterAnnotations");
size += 7 + 2 * (panns.Length - synthetics);
for (int i = panns.Length - 1; i >= synthetics; --i) {
size += panns[i] == null ? 0 : panns[i].getSize();
}
}
if (ClassReader.ANNOTATIONS && ipanns != null) {
cw.newUTF8("RuntimeInvisibleParameterAnnotations");
size += 7 + 2 * (ipanns.Length - synthetics);
for (int i = ipanns.Length - 1; i >= synthetics; --i) {
size += ipanns[i] == null ? 0 : ipanns[i].getSize();
}
}
if (attrs != null) {
size += attrs.getSize(cw, null, 0, -1, -1);
}
return size;
}
internal void put(ByteVector @out) {
int FACTOR = ClassWriter.TO_ACC_SYNTHETIC;
int mask = ACC_CONSTRUCTOR | Opcodes.ACC_DEPRECATED | ClassWriter.ACC_SYNTHETIC_ATTRIBUTE | ((access & ClassWriter.ACC_SYNTHETIC_ATTRIBUTE) / FACTOR);
@out.putShort(access & ~mask).putShort(name).putShort(desc);
if (classReaderOffset != 0) {
@out.putByteArray(cw.cr.b, classReaderOffset, classReaderLength);
return;
}
int attributeCount = 0;
if (code.length > 0) {
++attributeCount;
}
if (exceptionCount > 0) {
++attributeCount;
}
if ((access & Opcodes.ACC_SYNTHETIC) != 0) {
if ((cw.version & 0xFFFF) < Opcodes.V1_5 || (access & ClassWriter.ACC_SYNTHETIC_ATTRIBUTE) != 0) {
++attributeCount;
}
}
if ((access & Opcodes.ACC_DEPRECATED) != 0) {
++attributeCount;
}
if (ClassReader.SIGNATURES && signature != null) {
++attributeCount;
}
if (methodParameters != null) {
++attributeCount;
}
if (ClassReader.ANNOTATIONS && annd != null) {
++attributeCount;
}
if (ClassReader.ANNOTATIONS && anns != null) {
++attributeCount;
}
if (ClassReader.ANNOTATIONS && ianns != null) {
++attributeCount;
}
if (ClassReader.ANNOTATIONS && tanns != null) {
++attributeCount;
}
if (ClassReader.ANNOTATIONS && itanns != null) {
++attributeCount;
}
if (ClassReader.ANNOTATIONS && panns != null) {
++attributeCount;
}
if (ClassReader.ANNOTATIONS && ipanns != null) {
++attributeCount;
}
if (attrs != null) {
attributeCount += attrs.getCount();
}
@out.putShort(attributeCount);
if (code.length > 0) {
int size = 12 + code.length + 8 * handlerCount;
if (localVar != null) {
size += 8 + localVar.length;
}
if (localVarType != null) {
size += 8 + localVarType.length;
}
if (lineNumber != null) {
size += 8 + lineNumber.length;
}
if (stackMap != null) {
size += 8 + stackMap.length;
}
if (ClassReader.ANNOTATIONS && ctanns != null) {
size += 8 + ctanns.getSize();
}
if (ClassReader.ANNOTATIONS && ictanns != null) {
size += 8 + ictanns.getSize();
}
if (cattrs != null) {
size += cattrs.getSize(cw, code.data, code.length, maxStack, maxLocals);
}
@out.putShort(cw.newUTF8("Code")).putInt(size);
@out.putShort(maxStack).putShort(maxLocals);
@out.putInt(code.length).putByteArray(code.data, 0, code.length);
@out.putShort(handlerCount);
if (handlerCount > 0) {
Handler h = firstHandler;
while (h != null){
@out.putShort(h.start.position).putShort(h.end.position).putShort(h.handler.position).putShort(h.type);
h = h.next;
}
}
attributeCount = 0;
if (localVar != null) {
++attributeCount;
}
if (localVarType != null) {
++attributeCount;
}
if (lineNumber != null) {
++attributeCount;
}
if (stackMap != null) {
++attributeCount;
}
if (ClassReader.ANNOTATIONS && ctanns != null) {
++attributeCount;
}
if (ClassReader.ANNOTATIONS && ictanns != null) {
++attributeCount;
}
if (cattrs != null) {
attributeCount += cattrs.getCount();
}
@out.putShort(attributeCount);
if (localVar != null) {
@out.putShort(cw.newUTF8("LocalVariableTable"));
@out.putInt(localVar.length + 2).putShort(localVarCount);
@out.putByteArray(localVar.data, 0, localVar.length);
}
if (localVarType != null) {
@out.putShort(cw.newUTF8("LocalVariableTypeTable"));
@out.putInt(localVarType.length + 2).putShort(localVarTypeCount);
@out.putByteArray(localVarType.data, 0, localVarType.length);
}
if (lineNumber != null) {
@out.putShort(cw.newUTF8("LineNumberTable"));
@out.putInt(lineNumber.length + 2).putShort(lineNumberCount);
@out.putByteArray(lineNumber.data, 0, lineNumber.length);
}
if (stackMap != null) {
bool zip = (cw.version & 0xFFFF) >= Opcodes.V1_6;
@out.putShort(cw.newUTF8(zip ? "StackMapTable" : "StackMap"));
@out.putInt(stackMap.length + 2).putShort(frameCount);
@out.putByteArray(stackMap.data, 0, stackMap.length);
}
if (ClassReader.ANNOTATIONS && ctanns != null) {
@out.putShort(cw.newUTF8("RuntimeVisibleTypeAnnotations"));
ctanns.put(@out);
}
if (ClassReader.ANNOTATIONS && ictanns != null) {
@out.putShort(cw.newUTF8("RuntimeInvisibleTypeAnnotations"));
ictanns.put(@out);
}
if (cattrs != null) {
cattrs.put(cw, code.data, code.length, maxLocals, maxStack, @out);
}
}
if (exceptionCount > 0) {
@out.putShort(cw.newUTF8("Exceptions")).putInt(2 * exceptionCount + 2);
@out.putShort(exceptionCount);
for (int i = 0; i < exceptionCount; ++i) {
@out.putShort(exceptions[i]);
}
}
if ((access & Opcodes.ACC_SYNTHETIC) != 0) {
if ((cw.version & 0xFFFF) < Opcodes.V1_5 || (access & ClassWriter.ACC_SYNTHETIC_ATTRIBUTE) != 0) {
@out.putShort(cw.newUTF8("Synthetic")).putInt(0);
}
}
if ((access & Opcodes.ACC_DEPRECATED) != 0) {
@out.putShort(cw.newUTF8("Deprecated")).putInt(0);
}
if (ClassReader.SIGNATURES && signature != null) {
@out.putShort(cw.newUTF8("Signature")).putInt(2).putShort(cw.newUTF8(signature));
}
if (methodParameters != null) {
@out.putShort(cw.newUTF8("MethodParameters"));
@out.putInt(methodParameters.length + 1).putByte(methodParametersCount);
@out.putByteArray(methodParameters.data, 0, methodParameters.length);
}
if (ClassReader.ANNOTATIONS && annd != null) {
@out.putShort(cw.newUTF8("AnnotationDefault"));
@out.putInt(annd.length);
@out.putByteArray(annd.data, 0, annd.length);
}
if (ClassReader.ANNOTATIONS && anns != null) {
@out.putShort(cw.newUTF8("RuntimeVisibleAnnotations"));
anns.put(@out);
}
if (ClassReader.ANNOTATIONS && ianns != null) {
@out.putShort(cw.newUTF8("RuntimeInvisibleAnnotations"));
ianns.put(@out);
}
if (ClassReader.ANNOTATIONS && tanns != null) {
@out.putShort(cw.newUTF8("RuntimeVisibleTypeAnnotations"));
tanns.put(@out);
}
if (ClassReader.ANNOTATIONS && itanns != null) {
@out.putShort(cw.newUTF8("RuntimeInvisibleTypeAnnotations"));
itanns.put(@out);
}
if (ClassReader.ANNOTATIONS && panns != null) {
@out.putShort(cw.newUTF8("RuntimeVisibleParameterAnnotations"));
AnnotationWriter.put(panns, synthetics, @out);
}
if (ClassReader.ANNOTATIONS && ipanns != null) {
@out.putShort(cw.newUTF8("RuntimeInvisibleParameterAnnotations"));
AnnotationWriter.put(ipanns, synthetics, @out);
}
if (attrs != null) {
attrs.put(cw, null, 0, -1, -1, @out);
}
}
}
}
