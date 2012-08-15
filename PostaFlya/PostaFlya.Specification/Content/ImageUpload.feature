Feature: PARTICIPANT can upload IMAGE
	As a BROWSER in a PARTICIPANT role
	I want to be able to upload an IMAGE 
	so that it can be attached to a FLIER


Scenario: Image Upload Processing
	Given I am a BROWSER in PARTICIPANT ROLE 
	When I upload an IMAGE
	Then the IMAGE should be added to my list of IMAGES
	And the IMAGE status should be processing

Scenario: Image Upload Finished
	Given I am a BROWSER in PARTICIPANT ROLE 
	When I upload an IMAGE
	And The IMAGE is finished processing  
	Then the IMAGE should be added to my list of IMAGES
	And the IMAGE status should be ready

Scenario: Image Upload Failed
	Given I am a BROWSER in PARTICIPANT ROLE 
	When IMAGE processing fails 
	Then the IMAGE should be added to my list of IMAGES
	And the IMAGE status should be failed

Scenario: Image Upload Extracts Location Exif Data
	Given I am a BROWSER in PARTICIPANT ROLE 
	When I upload an IMAGE with Location Exif Data
	And The IMAGE is finished processing  
	Then the IMAGE should be added to my list of IMAGES
	And the IMAGE status should be ready
	And the IMAGE should have the Same Location as the uploaded IMAGE Exif Data

