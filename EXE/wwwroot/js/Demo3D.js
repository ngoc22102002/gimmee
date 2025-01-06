async function load3DModel() {
    const modelViewer = document.getElementById('viewer');
    await modelViewer.updateComplete; // Chờ model-viewer tải hoàn tất

    // Kiểm tra xem mô hình đã tải hoàn tất chưa
    if (!modelViewer.model) {
        console.error('Model not loaded yet');
        return;
    }

    // Tải mô hình GLTF từ đường dẫn và hiển thị nó trong model-viewer
    const gltfPath = '/3Dfile/scene.gltf';
    const gltfLoader = new THREE.GLTFLoader();

    gltfLoader.load(gltfPath, function (gltf) {
        const model = gltf.scene;

        // Thay đổi vật liệu của mô hình
        const textureLoader = new THREE.TextureLoader();
        textureLoader.load('/image/1/textures1.png', function (texture) {
            model.traverse(function (node) {
                if (node.isMesh) {
                    node.material.map = texture;
                    node.material.needsUpdate = true;
                }
            });
        });

        // Đặt mô hình vào model-viewer
        modelViewer.setModel(model);

        // Yêu cầu cập nhật lại model-viewer
        modelViewer.requestUpdate();
    });
}

// Thêm sự kiện để gọi hàm load3DModel khi cần thiết
document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('loadModelButton').addEventListener('click', load3DModel);
});
