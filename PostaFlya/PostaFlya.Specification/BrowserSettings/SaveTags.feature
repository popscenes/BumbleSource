Feature: Save Tags
	In order to be able to switch between tag sets for filtering	
	As a browser 
	I want to be able to save and delete tag sets

Scenario: View Saved Tags
	Given I have at least one TAG SET in my SAVED TAG SETS
	When i navigate to the BROWSER SAVED TAG SETS page 
	Then i should have a list of my SAVED TAG SETS

Scenario: Select Saved Tags
	Given I have at least one TAG SET in my SAVED TAG SETS
	When i Select a SAVED TAG SET 
	Then it should become my currently selected TAG SET

Scenario: Add Saved Tags
	Given I am a BROWSER in PARTICIPANT ROLE 
	And there are some TAGS set
	When I save the TAG SET to my Saved TAG SETs
	Then The TAG SET should be stored against my BROWSER

Scenario: Delete Saved Tags
	Given I have at least one TAG SET in my SAVED TAG SETS
	When i Delete a TAG SET from my SAVED TAG SETS 
	Then it should be removed from my SAVED TAG SETS
