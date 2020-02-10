using System;
using java.lang;

namespace org.objectweb.asm {
internal class Handler {
internal Label start;
internal Label end;
internal Label handler;
internal String desc;
internal int type;
internal Handler next;
static internal Handler remove(Handler h, Label start, Label end) {
if (h == null) {
return null;
}
else {
h.next = remove(h.next, start, end);
}
int hstart = h.start.position;
int hend = h.end.position;
int s = start.position;
int e = end == null ? Integer.MAX_VALUE : end.position;
if (s < hend && e > hstart) {
if (s <= hstart) {
if (e >= hend) {
h = h.next;
}
else {
h.start = end;
}
}
else if (e >= hend) {
h.end = start;
}
else {
Handler g = new Handler();
g.start = end;
g.end = h.end;
g.handler = h.handler;
g.desc = h.desc;
g.type = h.type;
g.next = h.next;
h.end = start;
h.next = g;
}
}
return h;
}
}
}
