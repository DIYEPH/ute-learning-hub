"use client";

import { Github } from "lucide-react";
import Link from "next/link";

export default function AboutPage() {
    return (
        <div className="max-w-xl mx-auto py-12 px-4 text-center">
            <div className="text-2xl font-bold mb-2">
                <h1 className="title">UTE Learning Hub</h1>
            </div>
            <p className="text-muted-foreground mb-6">
                Nền tảng chia sẻ tài liệu học tập và gợi ý nhóm cùng học tại
                <br />
                Trường Đại học Sư phạm Kỹ thuật Đà Nẵng
            </p>
            <Link
                href="https://github.com/DIYEPH/ute-learning-hub"
                target="_blank"
                className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground"
            >
                <Github className="w-4 h-4" />
                github.com/DIYEPH/ute-learning-hub
            </Link>
            <style jsx>{`
            .title {
                background-image: url('/images/bluesky.png');
                background-size: cover;
                background-clip: text;
                color: transparent;
                animation: animate 20s linear infinite;
            }
            @keyframes animate {
                to{
                    background-position-x: -500px;
                }
            }
            `}</style>
        </div>
    );
}