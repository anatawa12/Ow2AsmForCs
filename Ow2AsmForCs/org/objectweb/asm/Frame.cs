using System;
using java.lang;

namespace org.objectweb.asm {
internal class Frame {
internal const int DIM = unchecked((int)0xF0000000);
internal const int ARRAY_OF = 0x10000000;
internal const int ELEMENT_OF = unchecked((int)0xF0000000);
internal const int KIND = 0xF000000;
internal const int TOP_IF_LONG_OR_DOUBLE = 0x800000;
internal const int VALUE = 0x7FFFFF;
internal const int BASE_KIND = 0xFF00000;
internal const int BASE_VALUE = 0xFFFFF;
internal const int BASE = 0x1000000;
internal const int OBJECT = BASE | 0x700000;
internal const int UNINITIALIZED = BASE | 0x800000;
private const int LOCAL = 0x2000000;
private const int STACK = 0x3000000;
internal const int TOP = BASE | 0;
internal const int BOOLEAN = BASE | 9;
internal const int BYTE = BASE | 10;
internal const int CHAR = BASE | 11;
internal const int SHORT = BASE | 12;
internal const int INTEGER = BASE | 1;
internal const int FLOAT = BASE | 2;
internal const int DOUBLE = BASE | 3;
internal const int LONG = BASE | 4;
internal const int NULL = BASE | 5;
internal const int UNINITIALIZED_THIS = BASE | 6;
static readonly internal int[] SIZE;
static Frame(){
int i;
int[] b = new int[202];
String s = "EFFFFFFFFGGFFFGGFFFEEFGFGFEEEEEEEEEEEEEEEEEEEEDEDEDDDDD" + "CDCDEEEEEEEEEEEEEEEEEEEEBABABBBBDCFFFGGGEDCDCDCDCDCDCDCDCD" + "CDCEEEEDDDDDDDCDCDCEFEFDDEEFFDEDEEEBDDBBDDDDDDCCCCCCCCEFED" + "DDCDCDEEEEEEEEEEFEEEEEEDDEEDDEE";
for (i = 0; i < b.Length; ++i) {
b[i] = s.charAt(i) - 'E';
}
SIZE = b;
}
internal Label owner;
internal int[] inputLocals;
internal int[] inputStack;
private int[] outputLocals;
private int[] outputStack;
internal int outputStackTop;
private int initializationCount;
private int[] initializations;
internal void set(ClassWriter cw, int nLocal, Object[] local, int nStack, Object[] stack) {
int i = convert(cw, nLocal, local, inputLocals);
while (i < local.Length){
inputLocals[i++] = TOP;
}
int nStackTop = 0;
for (int j = 0; j < nStack; ++j) {
if (stack[j] == Opcodes.LONG || stack[j] == Opcodes.DOUBLE) {
++nStackTop;
}
}
inputStack = new int[nStack + nStackTop];
convert(cw, nStack, stack, inputStack);
outputStackTop = 0;
initializationCount = 0;
}
private static int convert(ClassWriter cw, int nInput, Object[] input, int[] output) {
int i = 0;
for (int j = 0; j < nInput; ++j) {
if (input[j] is int) {
output[i++] = BASE | (int)input[j];
if (input[j] == Opcodes.LONG || input[j] == Opcodes.DOUBLE) {
output[i++] = TOP;
}
}
else if (input[j] is String) {
output[i++] = type(cw, Type.getObjectType((String)input[j]).getDescriptor());
}
else {
output[i++] = UNINITIALIZED | cw.addUninitializedType("", ((Label)input[j]).position);
}
}
return i;
}
internal void set(Frame f) {
inputLocals = f.inputLocals;
inputStack = f.inputStack;
outputLocals = f.outputLocals;
outputStack = f.outputStack;
outputStackTop = f.outputStackTop;
initializationCount = f.initializationCount;
initializations = f.initializations;
}
private int get(int local) {
if (outputLocals == null || local >= outputLocals.Length) {
return LOCAL | local;
}
else {
int type = outputLocals[local];
if (type == 0) {
type = outputLocals[local] = LOCAL | local;
}
return type;
}
}
private void set(int local, int type) {
if (outputLocals == null) {
outputLocals = new int[10];
}
int n = outputLocals.Length;
if (local >= n) {
int[] t = new int[Math.Max(local + 1, 2 * n)];
SystemJ.arraycopy(outputLocals, 0, t, 0, n);
outputLocals = t;
}
outputLocals[local] = type;
}
private void push(int type) {
if (outputStack == null) {
outputStack = new int[10];
}
int n = outputStack.Length;
if (outputStackTop >= n) {
int[] t = new int[Math.Max(outputStackTop + 1, 2 * n)];
SystemJ.arraycopy(outputStack, 0, t, 0, n);
outputStack = t;
}
outputStack[outputStackTop++] = type;
int top = owner.inputStackTop + outputStackTop;
if (top > owner.outputStackMax) {
owner.outputStackMax = top;
}
}
private void push(ClassWriter cw, String desc) {
int type = Frame.type(cw, desc);
if (type != 0) {
push(type);
if (type == LONG || type == DOUBLE) {
push(TOP);
}
}
}
private static int type(ClassWriter cw, String desc) {
String t;
int index = desc.charAt(0) == '(' ? desc.indexOf(')') + 1 : 0;
switch (desc.charAt(index)) {
case 'V':
return 0;
case 'Z':
case 'C':
case 'B':
case 'S':
case 'I':
return INTEGER;
case 'F':
return FLOAT;
case 'J':
return LONG;
case 'D':
return DOUBLE;
case 'L':
t = desc.substring(index + 1, desc.length() - 1);
return OBJECT | cw.addType(t);
default:
int data;
int dims = index + 1;
while (desc.charAt(dims) == '['){
++dims;
}
switch (desc.charAt(dims)) {
case 'Z':
data = BOOLEAN;
break;
case 'C':
data = CHAR;
break;
case 'B':
data = BYTE;
break;
case 'S':
data = SHORT;
break;
case 'I':
data = INTEGER;
break;
case 'F':
data = FLOAT;
break;
case 'J':
data = LONG;
break;
case 'D':
data = DOUBLE;
break;
default:
t = desc.substring(dims + 1, desc.length() - 1);
data = OBJECT | cw.addType(t);
break;
}
return (dims - index) << 28 | data;
}
}
private int pop() {
if (outputStackTop > 0) {
return outputStack[--outputStackTop];
}
else {
return STACK | -(--owner.inputStackTop);
}
}
private void pop(int elements) {
if (outputStackTop >= elements) {
outputStackTop -= elements;
}
else {
owner.inputStackTop -= elements - outputStackTop;
outputStackTop = 0;
}
}
private void pop(String desc) {
char c = desc.charAt(0);
if (c == '(') {
pop((Type.getArgumentsAndReturnSizes(desc) >> 2) - 1);
}
else if (c == 'J' || c == 'D') {
pop(2);
}
else {
pop(1);
}
}
private void init(int var) {
if (initializations == null) {
initializations = new int[2];
}
int n = initializations.Length;
if (initializationCount >= n) {
int[] t = new int[Math.Max(initializationCount + 1, 2 * n)];
SystemJ.arraycopy(initializations, 0, t, 0, n);
initializations = t;
}
initializations[initializationCount++] = var;
}
private int init(ClassWriter cw, int t) {
int s;
if (t == UNINITIALIZED_THIS) {
s = OBJECT | cw.addType(cw.thisName);
}
else if ((t & (DIM | BASE_KIND)) == UNINITIALIZED) {
String type = cw.typeTable[t & BASE_VALUE].strVal1;
s = OBJECT | cw.addType(type);
}
else {
return t;
}
for (int j = 0; j < initializationCount; ++j) {
int u = initializations[j];
int dim = u & DIM;
int kind = u & KIND;
if (kind == LOCAL) {
u = dim + inputLocals[u & VALUE];
}
else if (kind == STACK) {
u = dim + inputStack[inputStack.Length - (u & VALUE)];
}
if (t == u) {
return s;
}
}
return t;
}
internal void initInputFrame(ClassWriter cw, int access, Type[] args, int maxLocals) {
inputLocals = new int[maxLocals];
inputStack = new int[0];
int i = 0;
if ((access & Opcodes.ACC_STATIC) == 0) {
if ((access & MethodWriter.ACC_CONSTRUCTOR) == 0) {
inputLocals[i++] = OBJECT | cw.addType(cw.thisName);
}
else {
inputLocals[i++] = UNINITIALIZED_THIS;
}
}
for (int j = 0; j < args.Length; ++j) {
int t = type(cw, args[j].getDescriptor());
inputLocals[i++] = t;
if (t == LONG || t == DOUBLE) {
inputLocals[i++] = TOP;
}
}
while (i < maxLocals){
inputLocals[i++] = TOP;
}
}
internal virtual void execute(int opcode, int arg, ClassWriter cw, Item item) {
int t1, t2, t3, t4;
switch (opcode) {
case Opcodes.NOP:
case Opcodes.INEG:
case Opcodes.LNEG:
case Opcodes.FNEG:
case Opcodes.DNEG:
case Opcodes.I2B:
case Opcodes.I2C:
case Opcodes.I2S:
case Opcodes.GOTO:
case Opcodes.RETURN:
break;
case Opcodes.ACONST_NULL:
push(NULL);
break;
case Opcodes.ICONST_M1:
case Opcodes.ICONST_0:
case Opcodes.ICONST_1:
case Opcodes.ICONST_2:
case Opcodes.ICONST_3:
case Opcodes.ICONST_4:
case Opcodes.ICONST_5:
case Opcodes.BIPUSH:
case Opcodes.SIPUSH:
case Opcodes.ILOAD:
push(INTEGER);
break;
case Opcodes.LCONST_0:
case Opcodes.LCONST_1:
case Opcodes.LLOAD:
push(LONG);
push(TOP);
break;
case Opcodes.FCONST_0:
case Opcodes.FCONST_1:
case Opcodes.FCONST_2:
case Opcodes.FLOAD:
push(FLOAT);
break;
case Opcodes.DCONST_0:
case Opcodes.DCONST_1:
case Opcodes.DLOAD:
push(DOUBLE);
push(TOP);
break;
case Opcodes.LDC:
switch (item.type) {
case ClassWriter.INT:
push(INTEGER);
break;
case ClassWriter.LONG:
push(LONG);
push(TOP);
break;
case ClassWriter.FLOAT:
push(FLOAT);
break;
case ClassWriter.DOUBLE:
push(DOUBLE);
push(TOP);
break;
case ClassWriter.CLASS:
push(OBJECT | cw.addType("java/lang/Class"));
break;
case ClassWriter.STR:
push(OBJECT | cw.addType("java/lang/String"));
break;
case ClassWriter.MTYPE:
push(OBJECT | cw.addType("java/lang/invoke/MethodType"));
break;
default:
push(OBJECT | cw.addType("java/lang/invoke/MethodHandle"));
break;
}
break;
case Opcodes.ALOAD:
push(get(arg));
break;
case Opcodes.IALOAD:
case Opcodes.BALOAD:
case Opcodes.CALOAD:
case Opcodes.SALOAD:
pop(2);
push(INTEGER);
break;
case Opcodes.LALOAD:
case Opcodes.D2L:
pop(2);
push(LONG);
push(TOP);
break;
case Opcodes.FALOAD:
pop(2);
push(FLOAT);
break;
case Opcodes.DALOAD:
case Opcodes.L2D:
pop(2);
push(DOUBLE);
push(TOP);
break;
case Opcodes.AALOAD:
pop(1);
t1 = pop();
push(ELEMENT_OF + t1);
break;
case Opcodes.ISTORE:
case Opcodes.FSTORE:
case Opcodes.ASTORE:
t1 = pop();
set(arg, t1);
if (arg > 0) {
t2 = get(arg - 1);
if (t2 == LONG || t2 == DOUBLE) {
set(arg - 1, TOP);
}
else if ((t2 & KIND) != BASE) {
set(arg - 1, t2 | TOP_IF_LONG_OR_DOUBLE);
}
}
break;
case Opcodes.LSTORE:
case Opcodes.DSTORE:
pop(1);
t1 = pop();
set(arg, t1);
set(arg + 1, TOP);
if (arg > 0) {
t2 = get(arg - 1);
if (t2 == LONG || t2 == DOUBLE) {
set(arg - 1, TOP);
}
else if ((t2 & KIND) != BASE) {
set(arg - 1, t2 | TOP_IF_LONG_OR_DOUBLE);
}
}
break;
case Opcodes.IASTORE:
case Opcodes.BASTORE:
case Opcodes.CASTORE:
case Opcodes.SASTORE:
case Opcodes.FASTORE:
case Opcodes.AASTORE:
pop(3);
break;
case Opcodes.LASTORE:
case Opcodes.DASTORE:
pop(4);
break;
case Opcodes.POP:
case Opcodes.IFEQ:
case Opcodes.IFNE:
case Opcodes.IFLT:
case Opcodes.IFGE:
case Opcodes.IFGT:
case Opcodes.IFLE:
case Opcodes.IRETURN:
case Opcodes.FRETURN:
case Opcodes.ARETURN:
case Opcodes.TABLESWITCH:
case Opcodes.LOOKUPSWITCH:
case Opcodes.ATHROW:
case Opcodes.MONITORENTER:
case Opcodes.MONITOREXIT:
case Opcodes.IFNULL:
case Opcodes.IFNONNULL:
pop(1);
break;
case Opcodes.POP2:
case Opcodes.IF_ICMPEQ:
case Opcodes.IF_ICMPNE:
case Opcodes.IF_ICMPLT:
case Opcodes.IF_ICMPGE:
case Opcodes.IF_ICMPGT:
case Opcodes.IF_ICMPLE:
case Opcodes.IF_ACMPEQ:
case Opcodes.IF_ACMPNE:
case Opcodes.LRETURN:
case Opcodes.DRETURN:
pop(2);
break;
case Opcodes.DUP:
t1 = pop();
push(t1);
push(t1);
break;
case Opcodes.DUP_X1:
t1 = pop();
t2 = pop();
push(t1);
push(t2);
push(t1);
break;
case Opcodes.DUP_X2:
t1 = pop();
t2 = pop();
t3 = pop();
push(t1);
push(t3);
push(t2);
push(t1);
break;
case Opcodes.DUP2:
t1 = pop();
t2 = pop();
push(t2);
push(t1);
push(t2);
push(t1);
break;
case Opcodes.DUP2_X1:
t1 = pop();
t2 = pop();
t3 = pop();
push(t2);
push(t1);
push(t3);
push(t2);
push(t1);
break;
case Opcodes.DUP2_X2:
t1 = pop();
t2 = pop();
t3 = pop();
t4 = pop();
push(t2);
push(t1);
push(t4);
push(t3);
push(t2);
push(t1);
break;
case Opcodes.SWAP:
t1 = pop();
t2 = pop();
push(t1);
push(t2);
break;
case Opcodes.IADD:
case Opcodes.ISUB:
case Opcodes.IMUL:
case Opcodes.IDIV:
case Opcodes.IREM:
case Opcodes.IAND:
case Opcodes.IOR:
case Opcodes.IXOR:
case Opcodes.ISHL:
case Opcodes.ISHR:
case Opcodes.IUSHR:
case Opcodes.L2I:
case Opcodes.D2I:
case Opcodes.FCMPL:
case Opcodes.FCMPG:
pop(2);
push(INTEGER);
break;
case Opcodes.LADD:
case Opcodes.LSUB:
case Opcodes.LMUL:
case Opcodes.LDIV:
case Opcodes.LREM:
case Opcodes.LAND:
case Opcodes.LOR:
case Opcodes.LXOR:
pop(4);
push(LONG);
push(TOP);
break;
case Opcodes.FADD:
case Opcodes.FSUB:
case Opcodes.FMUL:
case Opcodes.FDIV:
case Opcodes.FREM:
case Opcodes.L2F:
case Opcodes.D2F:
pop(2);
push(FLOAT);
break;
case Opcodes.DADD:
case Opcodes.DSUB:
case Opcodes.DMUL:
case Opcodes.DDIV:
case Opcodes.DREM:
pop(4);
push(DOUBLE);
push(TOP);
break;
case Opcodes.LSHL:
case Opcodes.LSHR:
case Opcodes.LUSHR:
pop(3);
push(LONG);
push(TOP);
break;
case Opcodes.IINC:
set(arg, INTEGER);
break;
case Opcodes.I2L:
case Opcodes.F2L:
pop(1);
push(LONG);
push(TOP);
break;
case Opcodes.I2F:
pop(1);
push(FLOAT);
break;
case Opcodes.I2D:
case Opcodes.F2D:
pop(1);
push(DOUBLE);
push(TOP);
break;
case Opcodes.F2I:
case Opcodes.ARRAYLENGTH:
case Opcodes.INSTANCEOF:
pop(1);
push(INTEGER);
break;
case Opcodes.LCMP:
case Opcodes.DCMPL:
case Opcodes.DCMPG:
pop(4);
push(INTEGER);
break;
case Opcodes.JSR:
case Opcodes.RET:
throw new RuntimeException("JSR/RET are not supported with computeFrames option");
case Opcodes.GETSTATIC:
push(cw, item.strVal3);
break;
case Opcodes.PUTSTATIC:
pop(item.strVal3);
break;
case Opcodes.GETFIELD:
pop(1);
push(cw, item.strVal3);
break;
case Opcodes.PUTFIELD:
pop(item.strVal3);
pop();
break;
case Opcodes.INVOKEVIRTUAL:
case Opcodes.INVOKESPECIAL:
case Opcodes.INVOKESTATIC:
case Opcodes.INVOKEINTERFACE:
pop(item.strVal3);
if (opcode != Opcodes.INVOKESTATIC) {
t1 = pop();
if (opcode == Opcodes.INVOKESPECIAL && item.strVal2.charAt(0) == '<') {
init(t1);
}
}
push(cw, item.strVal3);
break;
case Opcodes.INVOKEDYNAMIC:
pop(item.strVal2);
push(cw, item.strVal2);
break;
case Opcodes.NEW:
push(UNINITIALIZED | cw.addUninitializedType(item.strVal1, arg));
break;
case Opcodes.NEWARRAY:
pop();
switch (arg) {
case Opcodes.T_BOOLEAN:
push(ARRAY_OF | BOOLEAN);
break;
case Opcodes.T_CHAR:
push(ARRAY_OF | CHAR);
break;
case Opcodes.T_BYTE:
push(ARRAY_OF | BYTE);
break;
case Opcodes.T_SHORT:
push(ARRAY_OF | SHORT);
break;
case Opcodes.T_INT:
push(ARRAY_OF | INTEGER);
break;
case Opcodes.T_FLOAT:
push(ARRAY_OF | FLOAT);
break;
case Opcodes.T_DOUBLE:
push(ARRAY_OF | DOUBLE);
break;
default:
push(ARRAY_OF | LONG);
break;
}
break;
case Opcodes.ANEWARRAY:
String s = item.strVal1;
pop();
if (s.charAt(0) == '[') {
push(cw, '[' + s);
}
else {
push(ARRAY_OF | OBJECT | cw.addType(s));
}
break;
case Opcodes.CHECKCAST:
s = item.strVal1;
pop();
if (s.charAt(0) == '[') {
push(cw, s);
}
else {
push(OBJECT | cw.addType(s));
}
break;
default:
pop(arg);
push(cw, item.strVal1);
break;
}
}
internal bool merge(ClassWriter cw, Frame frame, int edge) {
bool changed = false;
int i, s, dim, kind, t;
int nLocal = inputLocals.Length;
int nStack = inputStack.Length;
if (frame.inputLocals == null) {
frame.inputLocals = new int[nLocal];
changed = true;
}
for (i = 0; i < nLocal; ++i) {
if (outputLocals != null && i < outputLocals.Length) {
s = outputLocals[i];
if (s == 0) {
t = inputLocals[i];
}
else {
dim = s & DIM;
kind = s & KIND;
if (kind == BASE) {
t = s;
}
else {
if (kind == LOCAL) {
t = dim + inputLocals[s & VALUE];
}
else {
t = dim + inputStack[nStack - (s & VALUE)];
}
if ((s & TOP_IF_LONG_OR_DOUBLE) != 0 && (t == LONG || t == DOUBLE)) {
t = TOP;
}
}
}
}
else {
t = inputLocals[i];
}
if (initializations != null) {
t = init(cw, t);
}
changed |= merge(cw, t, frame.inputLocals, i);
}
if (edge > 0) {
for (i = 0; i < nLocal; ++i) {
t = inputLocals[i];
changed |= merge(cw, t, frame.inputLocals, i);
}
if (frame.inputStack == null) {
frame.inputStack = new int[1];
changed = true;
}
changed |= merge(cw, edge, frame.inputStack, 0);
return changed;
}
int nInputStack = inputStack.Length + owner.inputStackTop;
if (frame.inputStack == null) {
frame.inputStack = new int[nInputStack + outputStackTop];
changed = true;
}
for (i = 0; i < nInputStack; ++i) {
t = inputStack[i];
if (initializations != null) {
t = init(cw, t);
}
changed |= merge(cw, t, frame.inputStack, i);
}
for (i = 0; i < outputStackTop; ++i) {
s = outputStack[i];
dim = s & DIM;
kind = s & KIND;
if (kind == BASE) {
t = s;
}
else {
if (kind == LOCAL) {
t = dim + inputLocals[s & VALUE];
}
else {
t = dim + inputStack[nStack - (s & VALUE)];
}
if ((s & TOP_IF_LONG_OR_DOUBLE) != 0 && (t == LONG || t == DOUBLE)) {
t = TOP;
}
}
if (initializations != null) {
t = init(cw, t);
}
changed |= merge(cw, t, frame.inputStack, nInputStack + i);
}
return changed;
}
private static bool merge(ClassWriter cw, int t, int[] types, int index) {
int u = types[index];
if (u == t) {
return false;
}
if ((t & ~DIM) == NULL) {
if (u == NULL) {
return false;
}
t = NULL;
}
if (u == 0) {
types[index] = t;
return true;
}
int v;
if ((u & BASE_KIND) == OBJECT || (u & DIM) != 0) {
if (t == NULL) {
return false;
}
else if ((t & (DIM | BASE_KIND)) == (u & (DIM | BASE_KIND))) {
if ((u & BASE_KIND) == OBJECT) {
v = (t & DIM) | OBJECT | cw.getMergedType(t & BASE_VALUE, u & BASE_VALUE);
}
else {
int vdim = ELEMENT_OF + (u & DIM);
v = vdim | OBJECT | cw.addType("java/lang/Object");
}
}
else if ((t & BASE_KIND) == OBJECT || (t & DIM) != 0) {
int tdim = (((t & DIM) == 0 || (t & BASE_KIND) == OBJECT) ? 0 : ELEMENT_OF) + (t & DIM);
int udim = (((u & DIM) == 0 || (u & BASE_KIND) == OBJECT) ? 0 : ELEMENT_OF) + (u & DIM);
v = Math.Min(tdim, udim) | OBJECT | cw.addType("java/lang/Object");
}
else {
v = TOP;
}
}
else if (u == NULL) {
v = (t & BASE_KIND) == OBJECT || (t & DIM) != 0 ? t : TOP;
}
else {
v = TOP;
}
if (u != v) {
types[index] = v;
return true;
}
return false;
}
}
}
