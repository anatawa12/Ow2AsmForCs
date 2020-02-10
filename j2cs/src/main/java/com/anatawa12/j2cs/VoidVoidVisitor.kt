package com.anatawa12.j2cs

import com.github.javaparser.ast.*
import com.github.javaparser.ast.body.*
import com.github.javaparser.ast.comments.BlockComment
import com.github.javaparser.ast.comments.JavadocComment
import com.github.javaparser.ast.comments.LineComment
import com.github.javaparser.ast.expr.*
import com.github.javaparser.ast.modules.*
import com.github.javaparser.ast.stmt.*
import com.github.javaparser.ast.type.*
import com.github.javaparser.ast.visitor.VoidVisitor
import javax.annotation.Generated

interface VoidVoidVisitor : VoidVisitor<Any?> {
    override fun visit(n: ReceiverParameter, arg: Any?) = visit(n)
    fun visit(n: ReceiverParameter)

    override fun visit(n: VarType, arg: Any?) = visit(n)
    fun visit(n: VarType)

    override fun visit(n: Modifier, arg: Any?) = visit(n)
    fun visit(n: Modifier)

    override fun visit(n: SwitchExpr, arg: Any?) = visit(n)
    fun visit(n: SwitchExpr)

    override fun visit(n: TextBlockLiteralExpr, arg: Any?) = visit(n)
    fun visit(n: TextBlockLiteralExpr)

    override fun visit(n: YieldStmt, arg: Any?) = visit(n)
    fun visit(n: YieldStmt)

    override fun visit(n: NodeList<*>, arg: Any?) = visit(n)
    fun visit(n: NodeList<*>)

    override fun visit(n: AnnotationDeclaration, arg: Any?) = visit(n)
    fun visit(n: AnnotationDeclaration)

    override fun visit(n: AnnotationMemberDeclaration, arg: Any?) = visit(n)
    fun visit(n: AnnotationMemberDeclaration)

    override fun visit(n: ArrayAccessExpr, arg: Any?) = visit(n)
    fun visit(n: ArrayAccessExpr)

    override fun visit(n: ArrayCreationExpr, arg: Any?) = visit(n)
    fun visit(n: ArrayCreationExpr)

    override fun visit(n: ArrayCreationLevel, arg: Any?) = visit(n)
    fun visit(n: ArrayCreationLevel)

    override fun visit(n: ArrayInitializerExpr, arg: Any?) = visit(n)
    fun visit(n: ArrayInitializerExpr)

    override fun visit(n: ArrayType, arg: Any?) = visit(n)
    fun visit(n: ArrayType)

    override fun visit(n: AssertStmt, arg: Any?) = visit(n)
    fun visit(n: AssertStmt)

    override fun visit(n: AssignExpr, arg: Any?) = visit(n)
    fun visit(n: AssignExpr)

    override fun visit(n: BinaryExpr, arg: Any?) = visit(n)
    fun visit(n: BinaryExpr)

    override fun visit(n: BlockComment, arg: Any?) = visit(n)
    fun visit(n: BlockComment)

    override fun visit(n: BlockStmt, arg: Any?) = visit(n)
    fun visit(n: BlockStmt)

    override fun visit(n: BooleanLiteralExpr, arg: Any?) = visit(n)
    fun visit(n: BooleanLiteralExpr)

    override fun visit(n: BreakStmt, arg: Any?) = visit(n)
    fun visit(n: BreakStmt)

    override fun visit(n: CastExpr, arg: Any?) = visit(n)
    fun visit(n: CastExpr)

    override fun visit(n: CatchClause, arg: Any?) = visit(n)
    fun visit(n: CatchClause)

    override fun visit(n: CharLiteralExpr, arg: Any?) = visit(n)
    fun visit(n: CharLiteralExpr)

    override fun visit(n: ClassExpr, arg: Any?) = visit(n)
    fun visit(n: ClassExpr)

    override fun visit(n: ClassOrInterfaceDeclaration, arg: Any?) = visit(n)
    fun visit(n: ClassOrInterfaceDeclaration)

    override fun visit(n: ClassOrInterfaceType, arg: Any?) = visit(n)
    fun visit(n: ClassOrInterfaceType)

    override fun visit(n: CompilationUnit, arg: Any?) = visit(n)
    fun visit(n: CompilationUnit)

    override fun visit(n: ConditionalExpr, arg: Any?) = visit(n)
    fun visit(n: ConditionalExpr)

    override fun visit(n: ConstructorDeclaration, arg: Any?) = visit(n)
    fun visit(n: ConstructorDeclaration)

    override fun visit(n: ContinueStmt, arg: Any?) = visit(n)
    fun visit(n: ContinueStmt)

    override fun visit(n: DoStmt, arg: Any?) = visit(n)
    fun visit(n: DoStmt)

    override fun visit(n: DoubleLiteralExpr, arg: Any?) = visit(n)
    fun visit(n: DoubleLiteralExpr)

    override fun visit(n: EmptyStmt, arg: Any?) = visit(n)
    fun visit(n: EmptyStmt)

    override fun visit(n: EnclosedExpr, arg: Any?) = visit(n)
    fun visit(n: EnclosedExpr)

    override fun visit(n: EnumConstantDeclaration, arg: Any?) = visit(n)
    fun visit(n: EnumConstantDeclaration)

    override fun visit(n: EnumDeclaration, arg: Any?) = visit(n)
    fun visit(n: EnumDeclaration)

    override fun visit(n: ExplicitConstructorInvocationStmt, arg: Any?) = visit(n)
    fun visit(n: ExplicitConstructorInvocationStmt)

    override fun visit(n: ExpressionStmt, arg: Any?) = visit(n)
    fun visit(n: ExpressionStmt)

    override fun visit(n: FieldAccessExpr, arg: Any?) = visit(n)
    fun visit(n: FieldAccessExpr)

    override fun visit(n: FieldDeclaration, arg: Any?) = visit(n)
    fun visit(n: FieldDeclaration)

    override fun visit(n: ForStmt, arg: Any?) = visit(n)
    fun visit(n: ForStmt)

    override fun visit(n: ForEachStmt, arg: Any?) = visit(n)
    fun visit(n: ForEachStmt)

    override fun visit(n: IfStmt, arg: Any?) = visit(n)
    fun visit(n: IfStmt)

    override fun visit(n: ImportDeclaration, arg: Any?) = visit(n)
    fun visit(n: ImportDeclaration)

    override fun visit(n: InitializerDeclaration, arg: Any?) = visit(n)
    fun visit(n: InitializerDeclaration)

    override fun visit(n: InstanceOfExpr, arg: Any?) = visit(n)
    fun visit(n: InstanceOfExpr)

    override fun visit(n: IntegerLiteralExpr, arg: Any?) = visit(n)
    fun visit(n: IntegerLiteralExpr)

    override fun visit(n: IntersectionType, arg: Any?) = visit(n)
    fun visit(n: IntersectionType)

    override fun visit(n: JavadocComment, arg: Any?) = visit(n)
    fun visit(n: JavadocComment)

    override fun visit(n: LabeledStmt, arg: Any?) = visit(n)
    fun visit(n: LabeledStmt)

    override fun visit(n: LambdaExpr, arg: Any?) = visit(n)
    fun visit(n: LambdaExpr)

    override fun visit(n: LineComment, arg: Any?) = visit(n)
    fun visit(n: LineComment)

    override fun visit(n: LocalClassDeclarationStmt, arg: Any?) = visit(n)
    fun visit(n: LocalClassDeclarationStmt)

    override fun visit(n: LongLiteralExpr, arg: Any?) = visit(n)
    fun visit(n: LongLiteralExpr)

    override fun visit(n: MarkerAnnotationExpr, arg: Any?) = visit(n)
    fun visit(n: MarkerAnnotationExpr)

    override fun visit(n: MemberValuePair, arg: Any?) = visit(n)
    fun visit(n: MemberValuePair)

    override fun visit(n: MethodCallExpr, arg: Any?) = visit(n)
    fun visit(n: MethodCallExpr)

    override fun visit(n: MethodDeclaration, arg: Any?) = visit(n)
    fun visit(n: MethodDeclaration)

    override fun visit(n: MethodReferenceExpr, arg: Any?) = visit(n)
    fun visit(n: MethodReferenceExpr)

    override fun visit(n: NameExpr, arg: Any?) = visit(n)
    fun visit(n: NameExpr)

    override fun visit(n: Name, arg: Any?) = visit(n)
    fun visit(n: Name)

    override fun visit(n: NormalAnnotationExpr, arg: Any?) = visit(n)
    fun visit(n: NormalAnnotationExpr)

    override fun visit(n: NullLiteralExpr, arg: Any?) = visit(n)
    fun visit(n: NullLiteralExpr)

    override fun visit(n: ObjectCreationExpr, arg: Any?) = visit(n)
    fun visit(n: ObjectCreationExpr)

    override fun visit(n: PackageDeclaration, arg: Any?) = visit(n)
    fun visit(n: PackageDeclaration)

    override fun visit(n: Parameter, arg: Any?) = visit(n)
    fun visit(n: Parameter)

    override fun visit(n: PrimitiveType, arg: Any?) = visit(n)
    fun visit(n: PrimitiveType)

    override fun visit(n: ReturnStmt, arg: Any?) = visit(n)
    fun visit(n: ReturnStmt)

    override fun visit(n: SimpleName, arg: Any?) = visit(n)
    fun visit(n: SimpleName)

    override fun visit(n: SingleMemberAnnotationExpr, arg: Any?) = visit(n)
    fun visit(n: SingleMemberAnnotationExpr)

    override fun visit(n: StringLiteralExpr, arg: Any?) = visit(n)
    fun visit(n: StringLiteralExpr)

    override fun visit(n: SuperExpr, arg: Any?) = visit(n)
    fun visit(n: SuperExpr)

    override fun visit(n: SwitchEntry, arg: Any?) = visit(n)
    fun visit(n: SwitchEntry)

    override fun visit(n: SwitchStmt, arg: Any?) = visit(n)
    fun visit(n: SwitchStmt)

    override fun visit(n: SynchronizedStmt, arg: Any?) = visit(n)
    fun visit(n: SynchronizedStmt)

    override fun visit(n: ThisExpr, arg: Any?) = visit(n)
    fun visit(n: ThisExpr)

    override fun visit(n: ThrowStmt, arg: Any?) = visit(n)
    fun visit(n: ThrowStmt)

    override fun visit(n: TryStmt, arg: Any?) = visit(n)
    fun visit(n: TryStmt)

    override fun visit(n: TypeExpr, arg: Any?) = visit(n)
    fun visit(n: TypeExpr)

    override fun visit(n: TypeParameter, arg: Any?) = visit(n)
    fun visit(n: TypeParameter)

    override fun visit(n: UnaryExpr, arg: Any?) = visit(n)
    fun visit(n: UnaryExpr)

    override fun visit(n: UnionType, arg: Any?) = visit(n)
    fun visit(n: UnionType)

    override fun visit(n: UnknownType, arg: Any?) = visit(n)
    fun visit(n: UnknownType)

    override fun visit(n: VariableDeclarationExpr, arg: Any?) = visit(n)
    fun visit(n: VariableDeclarationExpr)

    override fun visit(n: VariableDeclarator, arg: Any?) = visit(n)
    fun visit(n: VariableDeclarator)

    override fun visit(n: VoidType, arg: Any?) = visit(n)
    fun visit(n: VoidType)

    override fun visit(n: WhileStmt, arg: Any?) = visit(n)
    fun visit(n: WhileStmt)

    override fun visit(n: WildcardType, arg: Any?) = visit(n)
    fun visit(n: WildcardType)

    override fun visit(n: ModuleDeclaration, arg: Any?) = visit(n)
    fun visit(n: ModuleDeclaration)

    override fun visit(n: ModuleRequiresDirective, arg: Any?) = visit(n)
    fun visit(n: ModuleRequiresDirective)

    override fun visit(n: ModuleExportsDirective, arg: Any?) = visit(n)
    fun visit(n: ModuleExportsDirective)

    override fun visit(n: ModuleProvidesDirective, arg: Any?) = visit(n)
    fun visit(n: ModuleProvidesDirective)

    override fun visit(n: ModuleUsesDirective, arg: Any?) = visit(n)
    fun visit(n: ModuleUsesDirective)

    override fun visit(n: ModuleOpensDirective, arg: Any?) = visit(n)
    fun visit(n: ModuleOpensDirective)

    override fun visit(n: UnparsableStmt, arg: Any?) = visit(n)
    fun visit(n: UnparsableStmt)
}
