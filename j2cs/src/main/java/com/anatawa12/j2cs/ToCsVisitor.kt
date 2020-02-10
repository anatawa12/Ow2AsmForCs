package com.anatawa12.j2cs

import com.github.javaparser.ast.*
import com.github.javaparser.ast.body.*
import com.github.javaparser.ast.comments.BlockComment
import com.github.javaparser.ast.comments.JavadocComment
import com.github.javaparser.ast.comments.LineComment
import com.github.javaparser.ast.expr.*
import com.github.javaparser.ast.modules.*
import com.github.javaparser.ast.nodeTypes.NodeWithModifiers
import com.github.javaparser.ast.stmt.*
import com.github.javaparser.ast.type.*
import com.github.javaparser.utils.StringEscapeUtils
import java.util.*

class ToCsVisitor(val builder: Appendable) : VoidVoidVisitor, Appendable by builder {
    override fun visit(n: ArrayCreationLevel) = TODO()
    override fun visit(n: BlockComment) = TODO()
    override fun visit(n: JavadocComment) = TODO()
    override fun visit(n: LineComment) = TODO()
    override fun visit(n: MemberValuePair) = TODO()
    override fun visit(n: ModuleDeclaration) = TODO()
    override fun visit(n: ModuleExportsDirective) = TODO()
    override fun visit(n: ModuleOpensDirective) = TODO()
    override fun visit(n: ModuleProvidesDirective) = TODO()
    override fun visit(n: ModuleRequiresDirective) = TODO()
    override fun visit(n: ModuleUsesDirective) = TODO()
    override fun visit(n: NodeList<*>) = TODO()
    override fun visit(n: VariableDeclarator) = TODO()
    override fun visit(n: ReceiverParameter) = TODO()
    override fun visit(n: VarType) = TODO()
    override fun visit(n: Modifier) = TODO()
    override fun visit(n: SwitchExpr) = TODO()
    override fun visit(n: TextBlockLiteralExpr) = TODO()
    override fun visit(n: YieldStmt) = TODO()

    fun visitIdentifier(n: String) {
        if (n in csKeywords)
            append('@')
        append(n)
    }

    override fun visit(n: SimpleName) {
        visitIdentifier(n.identifier)
    }

    override fun visit(n: Name) {
        n.qualifier.kt()?.let {
            it.accept(this)
            append('.')
        }
        visitIdentifier(n.identifier)
    }

    //region types

    override fun visit(n: VoidType) = append("void").void()

    override fun visit(n: PrimitiveType): Unit = when (n.type) {
        PrimitiveType.Primitive.BOOLEAN -> append("bool").void()
        else -> append(n.type.asString()).void()
    }

    override fun visit(n: ClassOrInterfaceType) {
        n.scope.kt()?.run { accept(this@ToCsVisitor); append('.') }
        n.name.accept(this)
        n.typeArguments.kt()?.let { args ->
            append('<')
            args.joinTo(this, ", ") { it.accept(this).str() }
            append('>')
        }
    }

    override fun visit(n: ArrayType) {
        n.componentType.accept(this)
        append("[]")
    }

    override fun visit(n: IntersectionType) = TODO()
    override fun visit(n: WildcardType) {
        val extendedType = n.extendedType.kt()
        val superType = n.superType.kt()
        when {
            extendedType != null -> {
                append("/* ? extends */")
                extendedType.accept(this)
            }
            superType != null -> {
                append("/* ? super */")
                superType.accept(this)
            }
            else -> {
                append("/* ? (only) */ object")
            }
        }
    }
    override fun visit(n: UnknownType) = TODO()
    override fun visit(n: UnionType) = TODO()
    override fun visit(n: TypeParameter) = TODO()
//endregion

    //region entry point

    override fun visit(n: CompilationUnit) {
        appendln("using System;")
        appendln("using java.lang;")
        n.imports.map { getImportingPackage(it) }
            .distinctBy { it.second.asString() }
            .forEach {
                append("using ")
                if (it.first)
                    append("static ")
                it.second.accept(this)
                appendln(';')
            }

        appendln()
        if (n.packageDeclaration.kt() != null) {
            append("namespace ")
            n.packageDeclaration.get().name.accept(this)
            appendln(" {")
        }
        for (type in n.types) {
            type.accept(this)
        }
        if (n.packageDeclaration.kt() != null) {
            appendln("}")
        }
    }

    // isStatic, class name if static. if not, package name
    private fun getImportingPackage(n: ImportDeclaration): Pair<Boolean, Name> {
        val type = if (n.isAsterisk) n.name!! else n.name.qualifier.kt()!!
        return n.isStatic to type
    }

    override fun visit(n: ImportDeclaration) = TODO()
    override fun visit(n: PackageDeclaration) = TODO()

    //endregion

    //region declaration

    override fun visit(n: AnnotationDeclaration) = error("annotation declaration not supported")
    override fun visit(n: AnnotationMemberDeclaration) = error("annotation declaration not supported")

    private fun isInstanceOrInners(declaration: BodyDeclaration<*>): Boolean {
        if (declaration is ClassOrInterfaceDeclaration)
            return declaration.isInnerClass
        if (declaration is NodeWithModifiers<*>)
            return !declaration.modifiers.any { it.keyword == Modifier.Keyword.STATIC }
        if (declaration is InitializerDeclaration)
            return !declaration.isStatic
        TODO()
    }

    override fun visit(n: ClassOrInterfaceDeclaration) {
        val isGenerics = n.typeParameters.isNotEmpty()
        if (n.isInnerClass) {
            if (isGenerics) {
                TODO()
            } else {
                visitModifiers(n.modifiers, ModifierTarget.Class, n.parentNode)
                if (n.isInterface) append("interface ")
                else append("/*inner*/ class ")
                n.name.accept(this)
                visitTypeParameters(n.typeParameters)
                visitSuperTypes(n.extendedTypes + n.implementedTypes)
                visitTypeParameterBounds(n.typeParameters)
                appendln(" {")
                for (member in n.members) {
                    member.accept(this)
                }
                appendln("}")
            }
        } else {
            if (isGenerics) {
                // statics and non-inners
                visitModifiers(n.modifiers, ModifierTarget.Class, n.parentNode)
                append("static class ")
                n.name.accept(this)
                appendln(" {")
                for (member in n.members.filter { !isInstanceOrInners(it) }) {
                    member.accept(this)
                }
                appendln("}")
                appendln()
                // instance and inners
                visitModifiers(n.modifiers, ModifierTarget.Class, n.parentNode)
                if (n.isInterface) append("interface ")
                else append("class ")
                n.name.accept(this)
                visitTypeParameters(n.typeParameters)
                visitSuperTypes(n.extendedTypes + n.implementedTypes)
                visitTypeParameterBounds(n.typeParameters)
                appendln(" {")
                for (member in n.members.filter { isInstanceOrInners(it) }) {
                    member.accept(this)
                }
                appendln("}")
            } else {
                visitModifiers(n.modifiers, ModifierTarget.Class, n.parentNode)
                if (n.isInterface) append("interface ")
                else append("class ")
                n.name.accept(this)
                visitTypeParameters(n.typeParameters)
                visitSuperTypes(n.extendedTypes + n.implementedTypes)
                visitTypeParameterBounds(n.typeParameters)
                appendln(" {")
                for (member in n.members) {
                    member.accept(this)
                }
                appendln("}")
            }
        }
    }

    private fun visitSuperTypes(superTypes: List<ClassOrInterfaceType>) {
        if (superTypes.isEmpty()) return
        append(": ")
        superTypes.joinTo(this, ", ") { it.name.accept(this).str() }
    }

    private fun visitTypeParameters(typeParameters: NodeList<TypeParameter>) {
        if (typeParameters.isEmpty()) return
        append("<")
        typeParameters.joinTo(this, ", ") { it.name.accept(this).str() }
        append(">")
    }

    private fun visitTypeParameterBounds(typeParameters: NodeList<TypeParameter>) {
        if (typeParameters.isEmpty()) return
        for (typeParameter in typeParameters) {
            if (typeParameter.typeBound.isNotEmpty()) {
                appendln()
                append("where ")
                typeParameter.name.accept(this)
                append(" : ")
                typeParameter.typeBound.joinTo(this, ", ") { it.accept(this).str() }
            }
        }
    }

    override fun visit(n: ConstructorDeclaration) {
        val statements = n.body.statements
        val firstStatement = statements.firstOrNull()
        visitModifiers(n.modifiers, ModifierTarget.Constructor, n.parentNode)
        n.name.accept(this)
        visitTypeParameters(n.typeParameters)
        visitParameters(n.parameters)
        if (firstStatement is ExplicitConstructorInvocationStmt) {
            if (firstStatement.expression.kt() != null)
                TODO("super call with expression")
            if (!firstStatement.typeArguments.kt().isNullOrEmpty())
                TODO("super or this call with type parameters")
            if (firstStatement.isThis)
                append(": this(")
            else
                append(": base(")
            firstStatement.arguments.joinTo(this) {
                it.accept(this)
                str()
            }
            append(')')
            statements.removeAt(0)
        }
        visitTypeParameterBounds(n.typeParameters)
        append(' ')
        n.body.accept(this)
        appendln()
    }

    override fun visit(n: MethodDeclaration) {
        visitModifiers(n.modifiers, ModifierTarget.Method, n.parentNode)
        n.type.accept(this)
        append(' ')
        when (n.name.asString()) {
            "equals" -> append("Equals")
            "hashCode" -> append("GetHashCode")
            "toString" -> append("ToString")
            else -> n.name.accept(this)
        }
        visitTypeParameters(n.typeParameters)
        visitParameters(n.parameters)
        visitTypeParameterBounds(n.typeParameters)
        val body = n.body.kt()
        if (body == null) {
            appendln(';')
        } else {
            append(' ')
            body.accept(this)
        }
    }

    private fun visitParameters(parameters: NodeList<Parameter>) {
        append('(')
        parameters.joinTo(this) { it.accept(this).str() }
        append(')')
    }

    override fun visit(n: Parameter) {
        if (n.isVarArgs)
            append("params ")
        n.type.accept(this)
        if (n.isVarArgs)
            append("[]")
        append(' ')
        n.name.accept(this)
    }

    override fun visit(n: FieldDeclaration) {
        visitModifiers(n.modifiers, ModifierTarget.Field, n.parentNode)
        n.commonType.accept(this)
        append(' ')
        n.variables.joinTo(this,", \n") { variable ->
            variable.name.accept(this)
            val initializer = variable.initializer.kt()
            if (initializer != null) {
                append(" = ")
                initializer.accept(this)
            }
            str()
        }
        appendln(";")
    }

    override fun visit(n: InitializerDeclaration) {
        val name = (n.parentNode.kt() as TypeDeclaration<*>).name
        if (n.isStatic) append("static ")
        else append("public ")
        name.accept(this)
        append("()")
        n.body.accept(this)
    }

    override fun visit(n: EnumDeclaration) {
        TODO("enum")
    }

    override fun visit(n: EnumConstantDeclaration) {
        TODO("enum")
    }

    //endregion

    //region stmt

    override fun visit(n: LocalClassDeclarationStmt) {
        n.classDeclaration.accept(this)
    }

    override fun visit(n: ContinueStmt) {
        if (n.label.kt() == null) {
            appendln("continue;")
        } else {
            append("continue /* label: ")
            n.label.kt()!!.accept(this)
            appendln(" */;")
        }
    }

    override fun visit(n: BreakStmt) {
        if (n.label.kt() == null) {
            appendln("break;")
        } else {
            append("break /* label: ")
            n.label.kt()!!.accept(this)
            appendln(" */;")
        }
    }

    override fun visit(n: ExpressionStmt) {
        n.expression.accept(this)
        appendln(";")
    }

    override fun visit(n: LabeledStmt) {
        n.label.accept(this)
        appendln(":")
        n.statement.accept(this)
    }

    override fun visit(n: WhileStmt) {
        append("while (")
        n.condition.accept(this)
        append(")")
        n.body.accept(this)
    }

    override fun visit(n: ReturnStmt) {
        val expression = n.expression.kt()
        if (expression == null) {
            appendln("return;")
        } else {
            append("return ")
            expression.accept(this)
            appendln(";")
        }
    }

    override fun visit(n: EmptyStmt) {
        appendln(";")
    }

    override fun visit(n: ForEachStmt) {
        val variable = n.variable!!.variables.single()
        append("foreach (")
        variable.type.accept(this)
        append(' ')
        variable.name.accept(this)
        append(" in ")
        n.iterable.accept(this)
        append(") ")
        n.body.accept(this)
    }

    override fun visit(n: IfStmt) {
        append("if (")
        n.condition.accept(this)
        append(") ")
        n.thenStmt.accept(this)
        val elseStmt = n.elseStmt.kt();
        if (elseStmt != null) {
            append("else ")
            elseStmt.accept(this)
        }
    }

    override fun visit(n: DoStmt) {
        append("do ")
        n.body.accept(this)
        append("while (")
        n.condition.accept(this)
        appendln(");")
    }

    override fun visit(n: ThrowStmt) {
        append("throw ")
        n.expression.accept(this)
        appendln(';')
    }

    override fun visit(n: ForStmt) {
        append("for (")
        n.initialization.joinTo(this) {
            it.accept(this).str()
        }
        append("; ")
        n.compare.kt()?.accept(this)
        append("; ")
        n.update.joinTo(this) {
            it.accept(this).str()
        }
        append(") ")
        n.body.accept(this)
    }

    override fun visit(n: TryStmt) {
        val finallyBlock = n.finallyBlock.kt()
        if (n.resources.isEmpty()) {
            // try-catch only
            append("try ")
            n.tryBlock.accept(this)
            for (catchClause in n.catchClauses) {
                catchClause.accept(this)
            }
            if (finallyBlock != null) {
                append("finally ")
                finallyBlock.accept(this)
            }
        } else if (n.catchClauses.isEmpty() && finallyBlock == null) {
            // try-with-resources only
            n.resources.joinTo(this, "\n") { resource ->
                append("using (")
                resource.accept(this)
                append(")")
                str()
            }
            n.tryBlock.accept(this)
        } else {
            // try-catch-with-resources

            append("try {")

            n.resources.joinTo(this, "\n") { resource ->
                append("using (")
                resource.accept(this)
                append(")")
                str()
            }
            n.tryBlock.accept(this)
            append("}")
            for (catchClause in n.catchClauses) {
                catchClause.accept(this)
            }
            if (finallyBlock != null) {
                append("finally ")
                finallyBlock.accept(this)
            }
        }
    }

    override fun visit(n: CatchClause) {
        append("catch (")
        n.parameter.accept(this)
        append(") ")
        n.body.accept(this)
    }

    override fun visit(n: SwitchStmt) {
        append("switch (")
        n.selector.accept(this)
        appendln(") {")
        for (entry in n.entries) {
            val label = entry.labels.singleOrNull()
            if (label != null) {
                append("case ")
                label.accept(this)
                appendln(':')
            } else {
                appendln("default:")
            }

            for (statement in entry.statements) {
                statement.accept(this)
            }
        }
        appendln('}')
    }

    override fun visit(n: SynchronizedStmt) {
        append("lock (")
        n.expression.accept(this)
        append(") ")
        n.body.accept(this)
    }

    override fun visit(n: BlockStmt) {
        appendln('{')
        for (statement in n.statements) {
            statement.accept(this)
        }
        appendln('}')
    }

    ////////////////////////////////////////////////////////////////

    override fun visit(n: SwitchEntry) = TODO("impl in switch")

    override fun visit(n: ExplicitConstructorInvocationStmt) = TODO("must impl in constructor")

    override fun visit(n: AssertStmt) = TODO("unsupported")

    override fun visit(n: UnparsableStmt) = TODO("unsupported")

    //endregion

    //region expr

    private fun visitTypeArguments(typeArguments: NodeList<Type>?) {
        if (typeArguments.isNullOrEmpty()) return;
        append('<')
        typeArguments.joinTo(this) {
            it.accept(this)
            str()
        }
        append('>')
    }

    override fun visit(n: ArrayAccessExpr) {
        n.name.accept(this)
        append('[')
        n.index.accept(this)
        append(']')
    }

    override fun visit(n: ClassExpr) {
        append("typeof(")
        n.type.accept(this)
        append(")")
    }

    override fun visit(n: ArrayCreationExpr) {
        append("new ")
        n.elementType.accept(this)
        for (level in n.levels) {
            val dimension = level.dimension.kt()
            append('[')
            dimension?.accept(this)
            append(']')
        }
        n.initializer.kt()?.accept(this)
    }

    override fun visit(n: LambdaExpr) {
        visitParameters(n.parameters)
        append(" => ")
        n.body.accept(this)
    }

    override fun visit(n: ConditionalExpr) {
        n.condition.accept(this)
        append(" ? ")
        n.thenExpr.accept(this)
        append(" : ")
        n.elseExpr.accept(this)
    }

    override fun visit(n: MethodCallExpr) {
        n.scope.kt()?.let {
            it.accept(this)
            append('.')
        }
        n.name.accept(this)
        visitTypeArguments(n.typeArguments.kt())
        append('(')
        n.arguments.joinTo(this) {
            it.accept(this)
            str()
        }
        append(')')
    }

    override fun visit(n: AssignExpr) {
        n.target.accept(this)
        append(' ')
        append(n.operator.asString())
        append(' ')
        n.value.accept(this)
    }

    override fun visit(n: InstanceOfExpr) {
        n.expression.accept(this)
        append(" is ")
        n.type.accept(this)
    }

    override fun visit(n: ThisExpr) {
        append("this")
    }

    override fun visit(n: CastExpr) {
        append('(')
        n.type.accept(this)
        append(')')
        n.expression.accept(this)
    }

    override fun visit(n: NameExpr) {
        n.name.accept(this)
    }

    override fun visit(n: EnclosedExpr) {
        append('(')
        n.inner.accept(this)
        append(')')
    }

    override fun visit(n: VariableDeclarationExpr) {
        n.commonType.accept(this)
        append(' ')
        n.variables.joinTo(this) { variable ->
            variable.name.accept(this)
            variable.initializer.kt()?.let {
                append(" = ")
                it.accept(this)
            }
            str()
        }
    }

    override fun visit(n: NullLiteralExpr) {
        append("null")
    }

    override fun visit(n: BooleanLiteralExpr) {
        append(n.value.toString())
    }

    override fun visit(n: CharLiteralExpr) {
        append('\'')
        append(CSharpEscapeUtil.escape(n.asChar()))
        append('\'')
    }

    override fun visit(n: DoubleLiteralExpr) {
        append(n.value)
    }

    override fun visit(n: LongLiteralExpr) {
        append(n.value)
    }

    override fun visit(n: StringLiteralExpr) {
        append('"')
        append(CSharpEscapeUtil.escape(n.asString()))
        append('"')
    }

    override fun visit(n: IntegerLiteralExpr) {
        append(n.value)
    }

    override fun visit(n: ObjectCreationExpr) {
        append("new ")
        n.scope.kt()?.let {
            it.accept(this)
            append('.')
        }
        n.type.accept(this)
        visitTypeArguments(n.typeArguments.kt())
        append('(')
        n.arguments.joinTo(this) {
            it.accept(this)
            str()
        }
        append(')')
    }

    override fun visit(n: SuperExpr) {
        if (n.typeName.kt() != null) TODO("super with class")
        append("base")
    }

    override fun visit(n: UnaryExpr) {
        if (n.operator.isPrefix)
            append(n.operator.asString())
        n.expression.accept(this)
        if (n.operator.isPostfix)
            append(n.operator.asString())
    }

    override fun visit(n: BinaryExpr) {
        n.left.accept(this)
        append(' ')
        when (n.operator) {
            BinaryExpr.Operator.UNSIGNED_RIGHT_SHIFT -> append("/*>>>*/ >>")
            else -> append(n.operator.asString())
        }
        append(' ')
        n.right.accept(this)
    }

    override fun visit(n: FieldAccessExpr) {
        n.scope.accept(this)
        append('.')
        n.name.accept(this)
    }

    override fun visit(n: ArrayInitializerExpr) {
        append('{')
        n.values.joinTo(this) {
            it.accept(this)
            str()
        }
        append('}')
    }

    override fun visit(n: TypeExpr) {
        n.type.accept(this)
    }

    override fun visit(n: MethodReferenceExpr) = TODO("method reference")
    override fun visit(n: MarkerAnnotationExpr) = TODO("annotations")
    override fun visit(n: SingleMemberAnnotationExpr) = TODO("annotations")
    override fun visit(n: NormalAnnotationExpr) = TODO("annotations")

    //endregion

    //region modifiers

    val accessModiferKeywords = setOf(Modifier.Keyword.PUBLIC, Modifier.Keyword.PROTECTED, Modifier.Keyword.PRIVATE)

    fun visitModifiers(modifiersIn: NodeList<Modifier>, target: ModifierTarget, parent: Optional<Node>) {
        val classOrInterface = (parent.kt() as? ClassOrInterfaceDeclaration)
        val isInterface = classOrInterface?.isInterface == true
        val isFinalClass = classOrInterface?.modifiers?.any { it.keyword == Modifier.Keyword.FINAL } == true
        val modifiers = modifiersIn.map { it.keyword!! }
        for (modifier in modifiers) {
            when (modifier) {
                Modifier.Keyword.PUBLIC -> append("public ")
                Modifier.Keyword.PROTECTED -> append("protected ")
                Modifier.Keyword.PRIVATE -> append("private ")
                Modifier.Keyword.ABSTRACT -> append("abstract ")
                Modifier.Keyword.STATIC -> when (target) {
                    ModifierTarget.Class -> nop()
                    else -> append("static ")
                }
                Modifier.Keyword.FINAL -> when (target) {
                    ModifierTarget.Field -> append("readonly ")
                    ModifierTarget.Class -> append("sealed ")
                    else -> nop()
                }
                Modifier.Keyword.TRANSIENT -> TODO()
                Modifier.Keyword.VOLATILE -> append("volatile ")
                Modifier.Keyword.SYNCHRONIZED -> TODO()
                Modifier.Keyword.NATIVE -> TODO()
                Modifier.Keyword.STRICTFP -> TODO()
                Modifier.Keyword.TRANSITIVE -> TODO()
                Modifier.Keyword.DEFAULT -> TODO()
            }
        }
        if (modifiers.all { it !in accessModiferKeywords }) {
            // no access modifiers
            if (isInterface)
                append("public ")
            else
                append("internal ")
        }
        val keywords = setOf(Modifier.Keyword.FINAL, Modifier.Keyword.ABSTRACT, Modifier.Keyword.PRIVATE, Modifier.Keyword.STATIC)
        if (!isFinalClass && target == ModifierTarget.Method && modifiers.all { it !in keywords }) {
            // not final
            append("virtual ")
        }

        if (isInterface && target == ModifierTarget.Field && Modifier.Keyword.STATIC !in modifiers)
            append("static ")
    }

    enum class ModifierTarget {
        Class,
        Method,
        Constructor,
        Field,
    }

    //endregion
}
