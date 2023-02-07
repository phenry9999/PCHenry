let allMasksCheckbox = document.getElementById('toggle-all-masks');
allMasksCheckbox.addEventListener('click', toggleAllMasks);

function toggleAllMasks(){
	console.log("toggleAllMasks()");

	chrome.scripting.executeScript({
		target: {tabId: getTabId()},
		files: ["/mask_process.js"],
		func: toggleMask
	});
}