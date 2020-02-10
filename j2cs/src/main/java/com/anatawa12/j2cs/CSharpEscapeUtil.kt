package com.anatawa12.j2cs

object CSharpEscapeUtil {
    fun escape(char: Char) = escape(char.toString())

    fun escape(string: String) = buildString {
        for (char in string) {
            when (char) {
                '\u0027' -> append("\\\'")
                '\u0022' -> append("\\\"")
                '\u005C' -> append("\\\\")
                '\u0000' -> append("\\0")
                '\u0007' -> append("\\a")
                '\u0008' -> append("\\b")
                '\u000C' -> append("\\f")
                '\u000A' -> append("\\n")
                '\u000D' -> append("\\r")
                '\u0009' -> append("\\t")
                '\u000B' -> append("\\v")
                ' ' -> append(' ') // Category is Space but don't have to escape
                else -> when (GeneralCharCategory.get(char)) {
                    GeneralCharCategory.Letter -> append(char)
                    GeneralCharCategory.Mark -> append(char)
                    GeneralCharCategory.Number -> append(char)
                    GeneralCharCategory.Punctuation -> append(char)
                    GeneralCharCategory.Symbol -> append(char)
                    GeneralCharCategory.Separator -> backslashUEscape(this, char)
                    GeneralCharCategory.Other -> backslashUEscape(this, char)
                }
            }
        }
    }

    private fun backslashUEscape(builder: StringBuilder, char: Char) {
        builder.append("\\u")
        val hexString = char.toInt().toString(16)
        repeat(4 - hexString.length) { builder.append('0') }
        builder.append(hexString)
    }
}










