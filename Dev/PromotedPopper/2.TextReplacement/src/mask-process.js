console.log('Running any-mask ...');

let isMasked = false;
//let textToMask = 'google';
//const maskedText = 'REDACTED TEXT';
//let redactedRegex = new RegExp(maskedText, 'gi');

toggleMask();

function toggleMask(){
	console.log("toggleMask()");

	isMasked = !isMasked;
	let textToAdd;
	let textToFind;

	if(isMasked){
		//textToFind = 'skill\w*';
		textToFind = 'skill';
		textToAdd = 'REDACTED TEXT';
	}else{
		textToFind = 'REDACTED TEXT';
		textToAdd = 'skill';
	}

	let regex = new RegExp(textToFind, 'gi');
	Array.from(document.body.querySelectorAll('*'))
		.filter(e=> checkForMatch(e, textToFind))
		.forEach(e=>{
			console.log('Found an element:', e);
			if(e.innerHTML){
				//replace the keyboard globally, g (all instances instead of just
				//and match as case-insensitive, i
				e.innerHTML = e.innerHTML.replace(regex, textToAdd);
			}else if(e.value){
				e.value = e.value.replace(regex, textToAdd);
			}
		});
}

function checkForMatch(e, textToMask) {
	return (
	  (e.innerHTML && e.innerHTML.toLowerCase().indexOf(textToMask.toLowerCase()) > -1) ||
	  (e.value && e.value.toLowerCase().indexOf(textToMask.toLowerCase()) > -1)
	);
  }