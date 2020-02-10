plugins {
    java
    kotlin("jvm") version "1.3.61"
}

group = "org.example"
version = "1.0-SNAPSHOT"

repositories {
    mavenCentral()
}

dependencies {
    implementation(kotlin("stdlib-jdk8"))
    implementation("com.github.javaparser:javaparser-core:3.15.11")
    testCompile("junit", "junit", "4.12")
}

configure<JavaPluginConvention> {
    sourceCompatibility = JavaVersion.VERSION_1_8
}

val ow2AsmForCsRoot = projectDir.parentFile
val javaAsmSrcRoot = ow2AsmForCsRoot.resolve("java/src")
val csAsmSrcRoot = ow2AsmForCsRoot.resolve("Ow2AsmForCs")

val exclusions = listOf(
    "commons", "optimizer", "tree", "util", "xml"
)

tasks {
    compileKotlin {
        kotlinOptions.jvmTarget = "1.8"
    }
    compileTestKotlin {
        kotlinOptions.jvmTarget = "1.8"
    }

    register("generateCs", JavaExec::class.java) {
        classpath = configurations.runtimeClasspath.get()
        main = "com.anatawa12.j2cs.MainKt"
        args = listOf(javaAsmSrcRoot.toString(), csAsmSrcRoot.toString()) + exclusions
    }
}
