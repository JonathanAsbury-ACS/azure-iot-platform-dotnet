import React from 'react';
import '../manageDeviceGroups.scss';
import { compareByProperty } from 'utilities';
import {
  Btn,
  BtnToolbar
} from 'components/shared';
/* MODEL
export const toDeviceGroupModel = (deviceGroup = {}) => camelCaseReshape(deviceGroup, {
  'id': 'id',
  'displayName': 'displayName',
  'conditions': 'conditions',
  'eTag': 'eTag',
  'sortOrder': 'sortOrder',
  'isPinned': 'isPinned',
  'dirty': false, // track changes 
});
*/
export class DeviceGroups extends React.Component {

  // TODO: Remove constructor when args are passed in
  constructor(props) {
    super(props);

    var deviceGroups = this.props.deviceGroups;
    deviceGroups.forEach((group, index) => {
      group.changed = false;
      if(group.sortOrder === undefined)      
        group.sortOrder = index;
      if(group.isPinned === undefined)
        group.isPinned = false;
      if(group.previousGroup === undefined){
        group.previousGroup = (index===0)? '': deviceGroups[index].id
      }
    })

    this.state = {
        deviceGroups: deviceGroups    
    };
  }
  onPinnedChanged = (idx) => {
    const deviceGroups = this.state.deviceGroups;
    let pinChangeGroup = deviceGroups.splice(idx, 1)[0];    
    pinChangeGroup.isPinned = !pinChangeGroup.isPinned;        
    const pinnedItems = deviceGroups.filter(item => item.isPinned === true);
    
    deviceGroups.splice(pinnedItems.length, 0, pinChangeGroup); // insert at end of pinned group
    deviceGroups.forEach((group, index) => {
      group.sortOrder = index;
    })
    this.setState({ deviceGroups: deviceGroups }); 

  }

 onDragStart = (e, index) => {
    this.draggedItem = this.state.deviceGroups[index];
    e.dataTransfer.effectAllowed = "move";
  };


  onDragOver = index => {
    const draggedOverItem = this.state.deviceGroups[index];

    if (this.draggedItem === draggedOverItem) { return; } 
    let items = this.state.deviceGroups.filter(item => item !== this.draggedItem); 
    items.splice(index, 0, this.draggedItem); 

    let previousIdx = (index > 0)? index - 1 : 0;
    for(var i=0;i<items.length;i++){  
      items[i].sortOrder = i;
      if(i === index){
        items[i].isPinned = items[previousIdx].isPinned;
      }
    }

    this.setState({ deviceGroups: items }); 
  };

  onDragEnd = () => {
    this.draggedItem = null;
  };
  
  onApply = (event) => {
    event.preventDefault();
  }

  render(){
    // TODO: add deviceGroups to this.props
    const { t, onEditDeviceGroup } =  this.props;         
    // TODO: remove
    const { deviceGroups} = this.state;

    return(
      <div>
        <div className="device-group" >        
          <div className="group-title"> {t('deviceGroupsFlyout.deviceGroupName')}</div>
          <div className='list'>
            {
              deviceGroups.sort(compareByProperty('sortOrder', true)).map((deviceGroup, idx) => (
                <div className="item" data-index={idx} draggable onDragStart={e => this.onDragStart(e, idx)} onDragOver={() => this.onDragOver(idx)} onDragEnd={this.onDragEnd} >
                    {deviceGroup.isPinned
                        ? <img className="pinned"  src={require('./pushpin-closed.png')} onClick={() => this.onPinnedChanged(idx)} alt="Pinned Device Group" />            
                        : <img className="unpinned" src={require('./pushpin-open.png')} onClick={() => this.onPinnedChanged(idx)}  alt="Unpinned Device Group" />            
                    }        
                    <div className="title" key={idx} onClick={onEditDeviceGroup(deviceGroup)} >{deviceGroup.displayName}</div>
                    
                  </div>
                )
              )
            }
          </div>
        </div>
        <BtnToolbar> 
          <Btn primary={true} type="submit" onClick={() => this.onApply()}>{t('devices.flyouts.new.apply')}</Btn>
          <Btn onClick={() => this.onFlyoutClose('Devices_CancelClick')}>{t('devices.flyouts.new.cancel')}</Btn>
        </BtnToolbar>
    </div>
    )
  }
}
 
export default DeviceGroups;
