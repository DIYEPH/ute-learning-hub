"use client";

import * as React from "react";
import { Upload, X, ImageIcon } from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/src/components/ui/button";

interface ImageUploadProps {
    value?: File | null;
    onChange: (file: File | null) => void;
    disabled?: boolean;
    className?: string;
    aspectRatio?: "square" | "video" | "cover" | "auto";
    size?: "sm" | "md" | "lg";
    maxSizeMB?: number;
    placeholder?: string;
}

export function ImageUpload({
    value,
    onChange,
    disabled = false,
    className,
    aspectRatio = "cover",
    size = "sm",
    maxSizeMB = 5,
    placeholder = "Chọn ảnh",
}: ImageUploadProps) {
    const [preview, setPreview] = React.useState<string | null>(null);
    const [error, setError] = React.useState<string | null>(null);
    const inputRef = React.useRef<HTMLInputElement>(null);

    // Generate preview URL when file changes
    React.useEffect(() => {
        if (value) {
            const url = URL.createObjectURL(value);
            setPreview(url);
            return () => URL.revokeObjectURL(url);
        } else {
            setPreview(null);
        }
    }, [value]);

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        setError(null);

        if (!file) {
            onChange(null);
            return;
        }

        // Validate size
        if (file.size > maxSizeMB * 1024 * 1024) {
            setError(`Kích thước tối đa ${maxSizeMB}MB`);
            return;
        }

        // Validate type
        if (!file.type.startsWith("image/")) {
            setError("Vui lòng chọn file ảnh");
            return;
        }

        onChange(file);
    };

    const handleRemove = () => {
        onChange(null);
        if (inputRef.current) {
            inputRef.current.value = "";
        }
    };

    const aspectClasses = {
        square: "aspect-square",
        video: "aspect-video",
        cover: "aspect-[3/2]",
        auto: "",
    };

    const sizeClasses = {
        sm: "max-w-[200px]",
        md: "max-w-[300px]",
        lg: "max-w-[400px]",
    };

    const iconSizes = {
        sm: "h-6 w-6",
        md: "h-8 w-8",
        lg: "h-10 w-10",
    };

    return (
        <div className={cn("space-y-2", sizeClasses[size], className)}>
            <input
                ref={inputRef}
                type="file"
                accept="image/*"
                onChange={handleFileChange}
                className="hidden"
                disabled={disabled}
                id="image-upload"
            />

            {preview ? (
                // Preview mode - show full image
                <div className="relative group">
                    <div
                        className={cn(
                            "relative overflow-hidden border border-input bg-muted",
                            aspectClasses[aspectRatio]
                        )}
                    >
                        <img
                            src={preview}
                            alt="Preview"
                            className="w-full h-full object-contain"
                        />
                    </div>
                    {/* Overlay with actions */}
                    <div className="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-2">
                        <Button
                            type="button"
                            size="sm"
                            variant="secondary"
                            onClick={() => inputRef.current?.click()}
                            disabled={disabled}
                        >
                            <Upload className="h-4 w-4 mr-1" />
                            Đổi ảnh
                        </Button>
                        <Button
                            type="button"
                            size="sm"
                            variant="destructive"
                            onClick={handleRemove}
                            disabled={disabled}
                        >
                            <X className="h-4 w-4 mr-1" />
                            Xóa
                        </Button>
                    </div>
                    {/* File name */}
                    <div className="absolute bottom-0 left-0 right-0 bg-black/60 text-white text-xs px-2 py-1 truncate">
                        {value?.name}
                    </div>
                </div>
            ) : (
                // Upload mode - dropzone
                <label
                    htmlFor="image-upload"
                    className={cn(
                        "flex flex-col items-center justify-center gap-2 border-2 border-dashed cursor-pointer transition-colors",
                        aspectClasses[aspectRatio],
                        disabled
                            ? "border-muted bg-muted/50 cursor-not-allowed opacity-50"
                            : "border-muted-foreground/25 hover:border-primary hover:bg-accent/50"
                    )}
                >
                    <ImageIcon className={cn(iconSizes[size], "text-muted-foreground")} />
                    <span className="text-sm text-muted-foreground">{placeholder}</span>
                    <span className="text-xs text-muted-foreground">
                        Tối đa {maxSizeMB}MB
                    </span>
                </label>
            )}

            {error && (
                <p className="text-xs text-destructive">{error}</p>
            )}
        </div>
    );
}
