package com.anatawa12.j2cs

import com.github.javaparser.ast.visitor.Visitable
import com.github.javaparser.ast.visitor.VoidVisitor
import java.util.*

@Suppress("NOTHING_TO_INLINE")
inline fun <T: Any> T?.j(): Optional<T> = Optional.ofNullable(this)
@Suppress("NOTHING_TO_INLINE")
inline fun <T: Any> Optional<T>.kt(): T? = this.orElse(null)
@Suppress("NOTHING_TO_INLINE")
inline fun Visitable.accept(visitor: VoidVisitor<in Unit>) = accept(visitor, Unit)
@Suppress("NOTHING_TO_INLINE")
inline fun nop() {}
@Suppress("NOTHING_TO_INLINE")
inline fun <T> T.void() {}
@Suppress("NOTHING_TO_INLINE")
inline fun <T> T.str(): String = ""
