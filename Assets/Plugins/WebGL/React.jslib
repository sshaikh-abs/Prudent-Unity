mergeInto(LibraryManager.library, {
  handleGaugePoints: function (data) {
    window.dispatchReactUnityEvent("handleGaugePoints", UTF8ToString(data));
  },
  handleCompartmentSelection: function (data) {
    window.dispatchReactUnityEvent("handleCompartmentSelection", UTF8ToString(data));
  },
   handleCompartmentSelectionUID: function (data) {
    window.dispatchReactUnityEvent("handleCompartmentSelectionUID", UTF8ToString(data));
  },
  handlePartSelection: function(data) {
      window.dispatchReactUnityEvent("handlePartSelection", UTF8ToString(data));
  },
  handleMetadataInformation: function(data) {
      window.dispatchReactUnityEvent("handleMetadataInformation", UTF8ToString(data));
  },
  postVesselLoad: function(data) {
      window.dispatchReactUnityEvent("postVesselLoad", UTF8ToString(data));
  },
  handleCurrentStatename: function(data) {
      window.dispatchReactUnityEvent("handleCurrentStatename", UTF8ToString(data));
  },
  handleAttachDocument:function(data) {
      window.dispatchReactUnityEvent("handleAttachDocument", UTF8ToString(data));
  },
   handleError:function(data) {
      window.dispatchReactUnityEvent("handleError", UTF8ToString(data));
  },
  handleClickOnAttachIcon:function(data) {
      window.dispatchReactUnityEvent("handleClickOnAttachIcon", UTF8ToString(data));
  },
  handleDiminutionUnity:function(data) {
      window.dispatchReactUnityEvent("handleDiminutionUnity", UTF8ToString(data));
  },
  handleShowDocumentMetadata:function(data) {
      window.dispatchReactUnityEvent("handleShowDocumentMetadata", UTF8ToString(data));
  },
  handlePartHide:function(data) {
      window.dispatchReactUnityEvent("handlePartHide", UTF8ToString(data));
  },
  handleCapturedImagesForExport:function(data) {
      window.dispatchReactUnityEvent("handleCapturedImagesForExport", UTF8ToString(data));
  },
  handleAvgOriginalThickness:function(data) {
      window.dispatchReactUnityEvent("handleAvgOriginalThickness", UTF8ToString(data));
  },
  handleShowAll:function() {
      window.dispatchReactUnityEvent("handleShowAll");
  },
  handleZoomScrollValue:function(data) {
      window.dispatchReactUnityEvent("handleZoomScrollValue", UTF8ToString(data));
  },
  handleIsolatedObject:function(data) {
      window.dispatchReactUnityEvent("handleIsolatedObject", UTF8ToString(data));
  },
  handleSingleGaugePoint:function(data) {
      window.dispatchReactUnityEvent("handleSingleGaugePoint", UTF8ToString(data));
  },
  handleSingleGaugePointFlat:function(data) {
      window.dispatchReactUnityEvent("handleSingleGaugePointFlat", UTF8ToString(data));
  },
  handleGaugePointRemoval:function(data) {
      window.dispatchReactUnityEvent("handleGaugePointRemoval", UTF8ToString(data));
  },
    handleGaugePointRemovalFlat:function(data) {
      window.dispatchReactUnityEvent("handleGaugePointRemovalFlat", UTF8ToString(data));
  },
  handleCompartmentLoaded:function(data) {
      window.dispatchReactUnityEvent("handleCompartmentLoaded", UTF8ToString(data));
  },
  handlePlateName:function(data) {
      window.dispatchReactUnityEvent("handlePlateName", UTF8ToString(data));
  },
  handleCreateAnomaly:function(data) {
      window.dispatchReactUnityEvent("handleCreateAnomaly", UTF8ToString(data));
  },
  handleOnAutoGaugeCompleted:function() {
      window.dispatchReactUnityEvent("handleOnAutoGaugeCompleted");
  },
  handleAnomalyClicked:function(data){
    window.dispatchReactUnityEvent("handleAnomalyClicked", UTF8ToString(data));
  },
  handleEnterVesselView:function(){
    window.dispatchReactUnityEvent("handleEnterVesselView");
  },  
  handleRemoveAttachment:function(){
    window.dispatchReactUnityEvent("handleRemoveAttachment");
  },  
  handleEmptySpaceClick:function(){
    window.dispatchReactUnityEvent("handleEmptySpaceClick");
  },  
});