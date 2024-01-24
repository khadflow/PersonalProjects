/*var today = new Date();
var hourNow = today.getHours();
var greeting;

if (hourNow > 18) {
	greeting = "Good Evening!";
} else if (hourNow >= 12 && hourNow <= 18) {
	greeting = "Good Afternoon!";
} else {
	greeting = "Good Morning!";
}

document.write('<h3>' + greeting + "</h3>");

*/



// Class Declaration Examples

/*var accomplishments = {
	projectName: "Unity-EECS",
	role: "Programmer",
	description: "A 3D combat game based on mathematics and electrical engineering",
	year: 2023,
	language: "C#"
}*/
const Name = "Khadijah Flowers";

function reload() {
	location.reload();
}

function updateTime() {
	var today = new Date();
	document.write(today.toDateString() + " " + today.toTimeString());
}

updateTime();

// Loop
let program = ['Java', 'C#', 'Python', 'SQL', 'C', 'Javascript', 'HTML', 'CSS'];
for (let i = 0; i < program.length; i++) {
	//document.write(program[i] + " ");
}

// Constructor
function Accomplishment(projectName, role, description, year, language) {
	this.projectName = projectName;
	this.role = role;
	this.description = description;
	this.year = year;
	this.language = language;
}

var accomp = new Accomplishment("Unity-EECS", "Programmer", "Unity game", 2023, "C#");
//document.write(Name + " : ProjectName " + accomp.projectName + " : Role " + accomp.role + " : Description " + accomp.description + " : Year " + accomp.year + " : Language " + accomp.language);


// O(N) - FUNCTION / LOOP
function twoSum(array, sum) {
	let dict = {};
	for (let i = 0; i < array.length; i++) {
		let diff = sum - array[i];
		dict[array[i]] = diff;
	}

	for (let i = 0; i < array.length; i++) {
		if (dict[array[i]] in dict) {
			return [array[i], dict[array[i]], sum];
		}
	}
}

var arr = [1, 4, 7, 9, 10, 15];
var res = twoSum(arr, 25);
//alert(res);


// DOM