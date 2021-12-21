# Project 01 - Batch Rename

Thành viên nhóm:

MSSV | Họ và tên | Github
---- | --------- | ------
19120575 | Nguyễn Đức Mạnh | [manhnguyen1515](https://github.com/manhnguyen1515)
19120671 | Lê Nguyễn Nhất Thọ | [croissain](https://github.com/thole20042001)

## ⛳ Mục tiêu
Viết ứng dụng đổi tên nhiều tệp/thư mục.

## 📝 Các lưu ý cần thực hiện để chạy chương trình
- Nạp động các file .dll của từng rule trong thư mục CustomRuleLib vào BatchRename.
- Chạy file BatchRename.exe để thực thi ứng dụng.

## 🎯 Các chức năng đã hoàn thiện
## Yêu cầu cốt lõi (7 điểm)
- Nạp động các quy luật đổi tên từ file .dll bên ngoài.
- Chọn tất cả tệp/thư mục muốn đổi tên.
- Tạo bộ luật đổi tên cho các tệp/thư mục.
    - Mỗi luật được thêm từ menu.
    - Các tham số của luật có thể được chỉnh sửa.
- Áp dụng bộ các luật theo thứ tự lên các tệp/thư mục và đổi tên mới cho chúng.
- Lưu bộ các luật này thành preset để nhanh chóng tải lên nếu cần sử dụng.

### Yêu cầu kỹ thuật
- Áp dụng mẫu thiết kế: Abstract factory, prototype.
- Kiến trúc plugin
- Delegate & event

### Các luật đổi tên đã hoàn thiện
- Đổi tên phần mở rộng.
- Thêm bộ đếm vào sau tên tên tệp/thư mục.
- Bỏ ký tự khoảng trắng ở đầu và cuối tên tệp/thư mục.
- Thay thế các ký tự này thành ký tự khác.
- Thêm tiền tố, hậu tố cho tên tệp/thư mục.
- Chuyển đổi các ký tự thành LowerCase, UpperCase, PascalCase.


## 📈 Các chức năng phát triển thêm (3 điểm)
- Kéo thả tệp/thư mục vào, ứng dụng tự động thêm vào danh sách.
- Thêm đệ quy: chỉ cần chỉ định một thư mục, ứng dụng sẽ tự động quét và thêm tất cả các tệp trong thư mục đó.
- Xử lý trùng lặp.
- Có sử dụng Regex.
- Kiểm tra các ngoại lệ khi chỉnh sửa các tham số của quy tắc, xử lý validation.
- Cho phép người dùng xem trước kết quả trước khi xác nhận sửa tên.
- Tạo bản sao cho tất cả tệp và chuyển chúng sang thư mục khác thay vì xử lý đổi tên trực tiếp lên các tệp hiện tại.
- UI thân thiện với người dùng.

## 🎥 Video demo
https://www.youtube.com/watch?v=bdbqhHQMCCI&ab_channel=ManhNguyenDuc

> **Điểm đề xuất: 10**
