Feature: PARTICIPANT can Manage Setting
	As a BROWSER in a PARTICIPANT role
	I want to be able to Manage Tags and Locations 
	so that it can be included in a Selected easily

Scenario: View Saved Locations
	Given I am a BROWSER in PARTICIPANT ROLE
	When i navigate to the BROWSER SAVED LOCATIONS page 
	Then i should have a list of my SAVED LOCATIONS

Scenario: Select Saved Locations
	Given I am a BROWSER in PARTICIPANT ROLE 
	When i Select a SAVED LOCATION 
	Then it should become my currently selected LOCATION

Scenario: Add Saved Locations
	Given I am a BROWSER in PARTICIPANT ROLE 
	When i ADD a Location to my SAVED Locations 
	Then The LOCATION should be stored against my BROWSER

Scenario: Delete Saved Locations
	Given I am a BROWSER in PARTICIPANT ROLE 
	When i Delete a Location from my SAVED Locations 
	Then it should be removed from my SAVED LOCATIONS

Scenario: SetLocation
	Given I have navigated to the settings page to enter a DEFAULT LOCATION 
	When I enter a DEFAULT LOCATION 
	Then the DEFAULT LOCATION will be stored against the BROWSER

