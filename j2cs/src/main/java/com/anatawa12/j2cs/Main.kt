package com.anatawa12.j2cs

import com.github.javaparser.JavaParser
import com.github.javaparser.ast.CompilationUnit
import java.io.File
import java.io.InputStream
import java.lang.StringBuilder
import kotlin.system.exitProcess

fun main(args: Array<String>) {
    if (args.isEmpty()) {
        val unit = parse(System.`in`)
        val converter = ToCsVisitor(System.out)
        unit.accept(converter, Unit)
    } else if (args.size >= 2){
        val inDir = File(args[0])
        val outDir = File(args[1])
        val excludes = args.drop(2)
        System.err.println("excludes: ${excludes}")
        inDir.walkBottomUp()
            .filter { it.isFile }
            .filter { it.extension == "java" }
            .filter { it.relativeTo(inDir).path.let { path -> excludes.all { exclude -> exclude !in path } } }
            .forEach { inFile ->
                try {
                    var outFile = outDir.resolve(inFile.relativeTo(inDir))
                    outFile = outFile.parentFile.resolve(outFile.nameWithoutExtension + ".cs")
                    outFile.parentFile.mkdirs()

                    System.err.println("generating $inFile to $outFile")

                    inFile.inputStream().use { fileIn ->
                        outFile.writer().use { outWriter ->
                            val unit = parse(fileIn)
                            val converter = ToCsVisitor(outWriter)
                            unit.accept(converter, Unit)
                        }
                    }
                } catch (e: Throwable) {
                    throw Exception("during generating $inFile", e)
                }
            }
    } else {
        System.err.println("<input directoru> <output directory> <excludes> or no args for input file from stdin and output to stdout")
        exitProcess(1)
    }
}

fun parse(ins: InputStream): CompilationUnit {
    val parsed = JavaParser().parse(ins)
    if (parsed.problems.isNotEmpty()) {
        for (problem in parsed.problems) {
            System.err.println(problem)
        }
        throw Exception("parse error");
    }
    return parsed.result.kt()!!
}
