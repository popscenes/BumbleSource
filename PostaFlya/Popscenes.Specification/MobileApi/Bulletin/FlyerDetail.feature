@bulletinfeatures
Feature: FlyerDetail
	In order to find out more information
	As a browser
	I want to be able to request the details of a flyer 


Scenario: Get Flyer Detail returns flyer details
	Given There is an active flyer with the id ADC3D979-C0AB-474E-814E-E505A806447C
	When I perform a get request for the path mobileapi/gig/ADC3D979-C0AB-474E-814E-E505A806447C
	Then I should receive a http response with a status of 200
	And The content should have a response status of OK
	And The content should contain the detail for a flyer with the id ADC3D979-C0AB-474E-814E-E505A806447C
	
