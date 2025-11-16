// src/app/(auth)/layout.tsx
export default function AuthLayout({
    children,
  }: {
    children: React.ReactNode;
  }) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
        <div className="flex min-h-screen">
          {/* Left side - Branding */}
          <div className="hidden lg:flex lg:w-1/2 bg-gradient-to-br from-blue-600 to-purple-700 items-center justify-center">
            <div className="text-center text-white">
              <h1 className="text-4xl font-bold mb-4">UTE Learning Hub</h1>
              <p className="text-xl opacity-90">Nền tảng chia sẻ tài liệu học tập</p>
              <div className="mt-8">
                <div className="flex items-center justify-center space-x-8">
                  <div className="text-center">
                    <div className="text-3xl mb-2">📚</div>
                    <p>Tài liệu phong phú</p>
                  </div>
                  <div className="text-center">
                    <div className="text-3xl mb-2">👥</div>
                    <p>Học nhóm hiệu quả</p>
                  </div>
                  <div className="text-center">
                    <div className="text-3xl mb-2">💬</div>
                    <p>Chat thời gian thực</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
  
          {/* Right side - Auth forms */}
          <div className="w-full lg:w-1/2 flex items-center justify-center p-8">
            {children}
          </div>
        </div>
      </div>
    );
  }