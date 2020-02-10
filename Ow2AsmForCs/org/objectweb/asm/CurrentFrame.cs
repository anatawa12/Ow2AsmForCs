using System;
using java.lang;

namespace org.objectweb.asm {
internal class CurrentFrame: Frame {
internal virtual void execute(int opcode, int arg, ClassWriter cw, Item item) {
base.execute(opcode, arg, cw, item);
Frame successor = new Frame();
merge(cw, successor, 0);
set(successor);
owner.inputStackTop = 0;
}
}
}
