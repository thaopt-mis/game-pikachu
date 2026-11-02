# Game Pikachu (nối hình)

Đồ án cá nhân môn **Lập trình giao diện** - Đại học Mở TP. Hồ Chí Minh (04/2026).
Game nối hình cổ điển viết bằng **C# Windows Forms**: chọn 2 hình giống nhau, nếu nối được bằng đường gấp khúc **tối đa 2 lần rẽ** thì 2 hình biến mất. Xóa hết bàn cờ trước khi hết giờ để thắng.

<!-- Thêm ảnh/GIF game: sửa file này trên GitHub, kéo thả ảnh vào đây -->

## Tính năng
- **Bàn cờ tùy chỉnh**: chọn số hàng, số cột, số loại Pokémon và thời gian chơi; form kiểm tra dữ liệu (tổng số ô phải chẵn, số loại không vượt quá số cặp) bằng `ErrorProvider`
- **Tìm đường nối bằng BFS**: kiểm tra 2 hình có nối được trong ≤ 2 lần rẽ, cho phép đi vòng ra ngoài mép bàn cờ
- **Gợi ý**: tự tìm và tô sáng một cặp hình còn nối được
- **Tự phát hiện bế tắc**: khi không còn cặp nào nối được, game tự xáo trộn lại các hình còn lại (thử tối đa 200 lần để tránh treo)
- **Đổi chỗ (Swap)** thủ công, **tính điểm** (+10 mỗi cặp), **thanh thời gian** đếm ngược đổi màu
- Màn hình Menu, Hướng dẫn, Thắng/Thua; bật/tắt nhạc nền

## Thuật toán tìm đường (điểm chính của dự án)
Hàm `TimDuongNoi_22_Thao` trong `MainGame_22_Thao.cs`:
1. **Mở rộng ma trận** thêm 1 viền trống bao quanh để đường nối có thể đi vòng ngoài mép.
2. **BFS** với trạng thái `(dòng, cột, hướng, số lần rẽ)`; đổi hướng thì +1 lần rẽ, quá 2 lần thì bỏ nhánh đó.
3. **Phóng tia theo đường thẳng**: từ mỗi trạng thái, đi thẳng theo một hướng đến khi gặp vật cản, thay vì đi từng ô, nên số trạng thái cần duyệt giảm đáng kể.
4. Mảng đánh dấu 3 chiều `[dòng, cột, hướng]` lưu số lần rẽ ít nhất, chỉ duyệt lại khi tìm được đường ít rẽ hơn.

## Công nghệ
C#, .NET Framework 4.7.2, Windows Forms, Windows Media Player (nhạc nền), Visual Studio

## Cách chạy
1. **Code → Download ZIP**, giải nén.
2. Mở `22_Thao_N2_DoAnPikachu.sln` bằng Visual Studio (cần .NET Framework 4.7.2).
3. Nhấn **F5**.

## Ghi chú
Hình ảnh và nhạc Pokémon thuộc bản quyền của The Pokémon Company, chỉ dùng cho mục đích học tập.

## Tác giả
Phùng Thanh Thảo - Sinh viên Hệ thống Thông tin Quản lý
