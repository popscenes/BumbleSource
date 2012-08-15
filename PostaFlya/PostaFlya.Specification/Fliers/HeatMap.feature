Feature: HeatMap
	As a BROWSER 
	I want to be able to view fliers grouped on a heatmap
	So that i can see where the most active areas are

@mytag
Scenario: Get Heat Map Points
	Given I am a BROWSER in PARTICIPANT ROLE 
	When i view the HEAT MAP 
	Then the Map should show points grouped by FLIER LOCATION 
	And have the weight SUMMED
