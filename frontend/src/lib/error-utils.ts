export function getErrorMessage(err: any, defaultMsg = "Đã có lỗi xảy ra"): string {
    return err?.response?.data?.message || err?.response?.data?.title || err?.message || defaultMsg;
}