package com.anatawa12.j2cs

enum class GeneralCharCategory {
    Letter,
    Mark,
    Number,
    Punctuation,
    Symbol,
    Separator,
    Other,

    ;

    companion object {
        fun get(char: Char) = get(char.category)

        fun get(category: CharCategory) = when (category) {
            CharCategory.UPPERCASE_LETTER,
            CharCategory.LOWERCASE_LETTER,
            CharCategory.TITLECASE_LETTER,
            CharCategory.MODIFIER_LETTER,
            CharCategory.OTHER_LETTER -> Letter

            CharCategory.NON_SPACING_MARK,
            CharCategory.ENCLOSING_MARK,
            CharCategory.COMBINING_SPACING_MARK -> Mark

            CharCategory.DECIMAL_DIGIT_NUMBER,
            CharCategory.LETTER_NUMBER,
            CharCategory.OTHER_NUMBER -> Number

            CharCategory.CONNECTOR_PUNCTUATION,
            CharCategory.DASH_PUNCTUATION,
            CharCategory.START_PUNCTUATION,
            CharCategory.END_PUNCTUATION,
            CharCategory.INITIAL_QUOTE_PUNCTUATION,
            CharCategory.FINAL_QUOTE_PUNCTUATION,
            CharCategory.OTHER_PUNCTUATION -> Punctuation

            CharCategory.MATH_SYMBOL,
            CharCategory.CURRENCY_SYMBOL,
            CharCategory.MODIFIER_SYMBOL,
            CharCategory.OTHER_SYMBOL -> Symbol

            CharCategory.SPACE_SEPARATOR,
            CharCategory.LINE_SEPARATOR,
            CharCategory.PARAGRAPH_SEPARATOR -> Separator

            CharCategory.CONTROL,
            CharCategory.FORMAT,
            CharCategory.SURROGATE,
            CharCategory.PRIVATE_USE,
            CharCategory.UNASSIGNED -> Other
        }
    }
}
