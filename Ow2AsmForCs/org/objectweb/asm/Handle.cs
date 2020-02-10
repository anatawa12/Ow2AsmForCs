using System;
using java.lang;

namespace org.objectweb.asm {
public sealed class Handle {
readonly internal int tag;
readonly internal String owner;
readonly internal String name;
readonly internal String desc;
readonly internal bool itf;
public Handle(int tag, String owner, String name, String desc): this(tag, owner, name, desc, tag == Opcodes.H_INVOKEINTERFACE) {
}

public Handle(int tag, String owner, String name, String desc, bool itf) {
this.tag = tag;
this.owner = owner;
this.name = name;
this.desc = desc;
this.itf = itf;
}

public int getTag() {
return tag;
}
public String getOwner() {
return owner;
}
public String getName() {
return name;
}
public String getDesc() {
return desc;
}
public bool isInterface() {
return itf;
}
public bool Equals(Object obj) {
if (obj == this) {
return true;
}
if (!(obj is Handle)) {
return false;
}
Handle h = (Handle)obj;
return tag == h.tag && itf == h.itf && owner.equals(h.owner) && name.equals(h.name) && desc.equals(h.desc);
}
public int GetHashCode() {
return tag + (itf ? 64 : 0) + owner.hashCode() * name.hashCode() * desc.hashCode();
}
public String ToString() {
return owner + '.' + name + desc + " (" + tag + (itf ? " itf" : "") + ')';
}
}
}
