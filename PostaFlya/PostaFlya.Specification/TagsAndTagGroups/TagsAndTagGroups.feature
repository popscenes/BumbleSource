Feature: Addition
	In order to set the correct tags
	As i a user
	I need to have the correct tags return based on the website im on

@correctTags
Scenario: Display Correct Tags
	Given i have navigated to a page the requires Tag Selection
	Then then i should be able to choose the correct TAGS for the website
