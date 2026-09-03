document.addEventListener("DOMContentLoaded", function () {
    const maxImages = 10;
    const fileInput = document.getElementById("images-input");
    const imageStrip = document.getElementById("image-strip");
    const imageMetaInputs = document.getElementById("image-meta-inputs");

    const modal = document.getElementById("image-position-modal");
    const closeModalBtn = document.getElementById("close-image-modal");
    const savePositionBtn = document.getElementById("save-image-position");
    const previewImage = document.getElementById("position-preview-image");
    const positionXRange = document.getElementById("position-x-range");
    const positionYRange = document.getElementById("position-y-range");

    if (!fileInput || !imageStrip || !imageMetaInputs) {
        return;
    }

    let imageItems = [];
    let editingIndex = -1;

    function syncFileInput() {
        const dataTransfer = new DataTransfer();

        imageItems.forEach(item => {
            dataTransfer.items.add(item.file);
        });

        fileInput.files = dataTransfer.files;
    }

    function syncMetaInputs() {
        imageMetaInputs.innerHTML = "";

        imageItems.forEach((item, index) => {
            const xInput = document.createElement("input");
            xInput.type = "hidden";
            xInput.name = `Input.ImageCropPositions[${index}].X`;
            xInput.value = item.positionX;

            const yInput = document.createElement("input");
            yInput.type = "hidden";
            yInput.name = `Input.ImageCropPositions[${index}].Y`;
            yInput.value = item.positionY;

            imageMetaInputs.appendChild(xInput);
            imageMetaInputs.appendChild(yInput);
        });
    }

    function openPicker() {
        fileInput.click();
    }

    function render() {
        imageStrip.innerHTML = "";

        imageItems.forEach((item, index) => {
            const slot = document.createElement("div");
            slot.className = "portal-image-slot";

            const img = document.createElement("img");
            img.className = "portal-image-thumb";
            img.src = item.previewUrl;
            img.alt = `Afbeelding ${index + 1}`;
            img.style.objectPosition = `${item.positionX}% ${item.positionY}%`;

            const removeBtn = document.createElement("button");
            removeBtn.type = "button";
            removeBtn.className = "portal-image-remove-btn";
            removeBtn.setAttribute("aria-label", `Afbeelding ${index + 1} verwijderen`);
            removeBtn.textContent = "×";
            removeBtn.addEventListener("click", () => {
                URL.revokeObjectURL(item.previewUrl);
                imageItems.splice(index, 1);
                syncFileInput();
                syncMetaInputs();
                render();
            });

            const positionBtn = document.createElement("button");
            positionBtn.type = "button";
            positionBtn.className = "portal-image-position-btn";
            positionBtn.textContent = "Verplaats";
            positionBtn.addEventListener("click", () => {
                editingIndex = index;

                if (previewImage) {
                    previewImage.src = item.previewUrl;
                    previewImage.style.objectPosition = `${item.positionX}% ${item.positionY}%`;
                }

                if (positionXRange) {
                    positionXRange.value = item.positionX;
                }

                if (positionYRange) {
                    positionYRange.value = item.positionY;
                }

                if (modal) {
                    modal.hidden = false;
                }
            });

            slot.appendChild(img);
            slot.appendChild(removeBtn);
            slot.appendChild(positionBtn);
            imageStrip.appendChild(slot);
        });

        const emptyCount = Math.max(0, maxImages - imageItems.length);

        for (let i = 0; i < emptyCount; i++) {
            const emptySlot = document.createElement("button");
            emptySlot.type = "button";
            emptySlot.className = "portal-image-slot portal-image-slot--empty";
            emptySlot.setAttribute("aria-label", "Afbeelding toevoegen");
            emptySlot.innerHTML = `<span class="portal-image-slot-plus">+</span>`;
            emptySlot.addEventListener("click", openPicker);
            imageStrip.appendChild(emptySlot);
        }
    }

    function updatePreviewPosition() {
        if (!previewImage || !positionXRange || !positionYRange) {
            return;
        }

        previewImage.style.objectPosition = `${positionXRange.value}% ${positionYRange.value}%`;
    }

    fileInput.addEventListener("change", (event) => {
        const selectedFiles = Array.from(event.target.files || []);
        if (selectedFiles.length === 0) {
            return;
        }

        const availableSlots = maxImages - imageItems.length;
        const filesToAdd = selectedFiles.slice(0, availableSlots);

        filesToAdd.forEach(file => {
            imageItems.push({
                file,
                previewUrl: URL.createObjectURL(file),
                positionX: 50,
                positionY: 50
            });
        });

        syncFileInput();
        syncMetaInputs();
        render();
    });

    if (positionXRange) {
        positionXRange.addEventListener("input", updatePreviewPosition);
    }

    if (positionYRange) {
        positionYRange.addEventListener("input", updatePreviewPosition);
    }

    if (savePositionBtn) {
        savePositionBtn.addEventListener("click", () => {
            if (editingIndex < 0 || !imageItems[editingIndex]) {
                return;
            }

            imageItems[editingIndex].positionX = Number(positionXRange.value);
            imageItems[editingIndex].positionY = Number(positionYRange.value);

            syncMetaInputs();
            render();

            if (modal) {
                modal.hidden = true;
            }

            editingIndex = -1;
        });
    }

    if (closeModalBtn) {
        closeModalBtn.addEventListener("click", () => {
            if (modal) {
                modal.hidden = true;
            }

            editingIndex = -1;
        });
    }

    if (modal) {
        modal.addEventListener("click", (event) => {
            if (event.target === modal) {
                modal.hidden = true;
                editingIndex = -1;
            }
        });
    }

    render();
});